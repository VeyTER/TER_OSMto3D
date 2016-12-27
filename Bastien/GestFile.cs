using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GestFile
{
	public GestFile()
	{
	}
    // liste des nodes ( structure de donnée )
    private ArrayList nodes = new ArrayList();
    // liste des groupes de nodes ( structure de donnée )
    public static ArrayList nodeGroups = new ArrayList();
    // compteur de nodes et groupe de nodes respectivements
    private int counter;
    private int buildingCounter;
    // coordonnées min et max de la carte
    private float minlat, maxlat, minlon, maxlon;
    // chemin d'acces et nom du fichier
    private string path = @"./Assets/";
    public string fileName = "map";
    // listes des gameObjects créés dans la scene
    public static GameObject[] mainWalls;
    public static GameObject[] mainNodes;
    public static GameObject panel = null;


    // Methode readFileOSM :
    // permet de lire ligne par ligne le fichier et d'en extraire les données
    // Parametre :
    //      - nomMap : nom du fichier dans lequel se trouvent les infos
    public void readFileOSM(string nameMap)
    {
        counter = 0;
        buildingCounter = 0;
        string line;

        long id = 0;
        float lat = 0f;
        float lon = 0f;

        // Read the file and display it line by line.
        System.IO.StreamReader file = new System.IO.StreamReader(path + "Maps/" + nomMap + ".osm");

        // on commence par repertorier toutes les nodes de la  carte
        while ((line = file.ReadLine()) != null)
        {
            // on recupère les extremites
            if (line.Contains("<bounds"))
            {
                minlat = float.Parse(line.Substring(line.IndexOf("minlat=") + 8, 9)) * 1000f;
                maxlat = float.Parse(line.Substring(line.IndexOf("maxlat=") + 8, 9)) * 1000f;
                minlon = float.Parse(line.Substring(line.IndexOf("minlon=") + 8, 9)) * 1000f;
                maxlon = float.Parse(line.Substring(line.IndexOf("maxlon=") + 8, 9)) * 1000f;
            }


            // on recupère les nodes
            if (line.Contains("<node"))
            {
                id = long.Parse(line.Substring(line.IndexOf("id=") + 4, line.IndexOf("\" visible") - line.IndexOf("id=") - 4));
                lat = float.Parse(line.Substring(line.IndexOf("lat=") + 5, 9));
                lon = float.Parse(line.Substring(line.IndexOf("lon=") + 5, 9));

                nodes.Add(new Node(id, lat, lon));
                counter++;
            }

            // on recupere les batiments
            if (line.Contains("<way"))
            {

                // on créé un nouveau groupement de nodes
                NodeGroup current = new NodeGroup(long.Parse(line.Substring(line.IndexOf("id=") + 4, line.IndexOf("\" visible") - line.IndexOf("id=") - 4)));
                // on remplit ce groupement de node
                line = file.ReadLine();

                while (!line.Contains("</way>"))
                {
                    // cas d'ajout d'un node
                    if (line.Contains("<nd"))
                    {
                        // on recupere l'id du node
                        long reference = long.Parse(line.Substring(line.IndexOf("ref=") + 5, line.IndexOf("\"/>") - line.IndexOf("ref=") - 5));

                        foreach (Node n in nodes)
                        {
                            // on ajoute le node a la lsite de ceux qui compose le node groupe
                            if (n.id == reference)
                            {
                                current.addNode(n);
                            }
                        }
                    }

                    if (line.Contains("<tag"))
                    {
                        string key = line.Substring(line.IndexOf("k=") + 3, line.IndexOf("\" v=") - line.IndexOf("k=") - 3);
                        string value = line.Substring(line.IndexOf("v=") + 3, line.IndexOf("\"/>") - line.IndexOf("v=") - 3);
                        // on ajoute le tag
                        current.addTag(key, value);
                        if (key.Equals("building") && value.Equals("yes"))
                        {
                            buildingCounter++;
                        }
                    }

                    line = file.ReadLine();
                }
                nodeGroups.Add(current);
            }

        }

        file.Close();
        //Debug.Log ("There were "+ counter +" nodes.");
    }

    //Methode createResumeFile
    // créé un fichier propre ou les informations sont résumées
    // Parametre :
    //      - nameFile : nom du fichier ou l'on inscrit les informations 
    public void createResumeFile(string nameFile)
    {

        string pathString = path + "MapsResumed/" + nomFile + "Resumed.osm";
        string buildingName;

        //on créé le fichier et on va écrire dedans
        System.IO.StreamWriter file = new System.IO.StreamWriter(pathString);
        file.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");


        //Supression pour avoir le nouveau format 
        /*// on réécrit la liste des nodes avec les coordonnées recalculées
        file.WriteLine("\t<nodeList number=\"" + counter + "\" >");
        foreach (Node n in nodes)
        {
            file.WriteLine("\t\t<node id=\"" + n.id + "\" lat=\"" + n.latitude + "\" lon=\"" + n.longitude + "\" />");
        }
        file.WriteLine("\t</nodeList>");
        */

        // on réécrit la liste des batiments
        file.WriteLine("\t<earth\>");
        file.WriteLine("\t\t<buildingList number=\"" + buildingCounter + "\" >");
        foreach (NodeGroup ngp in nodeGroups)
        {
            if (ngp.isBuilding())
            {
                //on récupère le nom du batiment
                buildingName = ngp.getName();

                /*if (ngp.tags.ContainsKey("name"))
                {
                    buildingName = ngp.tags["name"].ToString();
                }*/

                file.WriteLine("\t\t\t<building id=\"" + ngp.id + "\" name=\"" + buildingName + "\" >");
                foreach (Node n in ngp.nodes)
                {
                    file.WriteLine("\t\t\t\t<node id=\"" + n.id + "\" >");
                }
                file.WriteLine("\t\t\t</building id=\"" + ngp.id + "\" >");
            }
        }
        file.WriteLine("\t\t</buildingList>");

        file.WriteLine("</xml>");

        file.Close();
    }

    //Methode createSettingsFile
    // créé un fichier de parametre
    // Parametre :
    //      - nameFile : nom du fichier ou l'on inscrit les informations 
    public void createSettingsFile(string nameFile)
    {
        string pathString = path + "MapsSettings/" + nomFile + "Settings.osm";
        string buildingName;

        //on créé le fichier et on va écrire dedans
        System.IO.StreamWriter file = new System.IO.StreamWriter(pathString);
        file.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");


        // on réécrit la liste des batiments en mettant des options par défaut
        file.WriteLine("\t<buildingList number=\"" + buildingCounter + "\" >");
        foreach (NodeGroup ngp in nodeGroups)
        {
            if (ngp.isBuilding())
            {
                //on récupère le nom du batiment
                buildingName = "unknown";
                if (ngp.tags.ContainsKey("name"))
                {
                    buildingName = ngp.tags["name"].ToString();
                }

                file.WriteLine("\t\t<building id=\"" + ngp.id + "\" name=\"" + buildingName + "\" floors=\"1\" >");
            }
        }

        file.WriteLine("\t</buildingList>");

        file.WriteLine("</xml>");

        file.Close();
    }

    //Methode readSettingsFile
    // lit un fichier setting
    // Parametre :
    //      - nameFile : nom du fichier à lire
    public void readSettingsFile(string nameFile)
    {
        string pathString = path + "MapsSettings/" + nomFile + "Settings.osm";
        string line;
        int etages = 5;
        long id;

        //on lit le fichier de configuration
        System.IO.StreamReader file = new System.IO.StreamReader(pathString);

        while ((line = file.ReadLine()) != null)
        {
            // on recupère les nodes
            if (line.Contains("<building "))
            {
                // on recupere l'id du batiment et son nombre d'etages
                id = long.Parse(line.Substring(line.IndexOf("id=") + 4, line.IndexOf("\" name") - line.IndexOf("id=") - 4));
                etages = int.Parse(line.Substring(line.IndexOf("floors=") + 8, line.IndexOf("\" >") - line.IndexOf("floors=") - 8));

                //on met a jour la structure

                NodeGroup ng = new NodeGroup(id);
                foreach (NodeGroup ngp in nodeGroups)
                {
                    if (ngp.equals(ng))
                    {
                        ngp.addTag("etages", etages.ToString());
                        ngp.addTag("temperature", (int)Random.Range(0, 35) + "°C");
                        ngp.addTag("humidité", (int)Random.Range(1, 10) + "%");
                    }
                }
            }
        }

        file.Close();
    }

    public void readResumeFile(string nameFile)
    {

    }
}
