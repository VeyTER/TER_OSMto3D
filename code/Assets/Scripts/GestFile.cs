using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GestFile
{

    // compteur de nodes et groupe de nodes respectivements
    public int counter;
    public int buildingCounter;
    // coordonnées min et max de la carte
    public float minlat, maxlat, minlon, maxlon;
   
    // chemin d'acces et nom du fichier par deffaut
    private string path = @"./Assets/";
    private string fileName = "map";


    // Arraylist permettant de stocker les balises
    private ArrayList cou = new ArrayList();
    private ArrayList reg = new ArrayList();
    private ArrayList tow = new ArrayList();
    private ArrayList dis = new ArrayList();


    //Constructeur par deffaut
    public GestFile()
    {
        
    }

    //Constructeur 
    public GestFile(string path, string fileName )
    {
        this.path = path;
        this.fileName = fileName;
    }

    //Accesseur pour le nom des fichiers
    public void setPath(string path)
    {
        this.path = path;
    }
    public string getPath()
    {
        return this.path;
    }

    public void setFileName(string fileName)
    {
        this.fileName = fileName;
    }
    public string getFileName()
    {
        return this.fileName;

    }

    /// <summary>
    /// Methode readFileOSM :
    /// Permet d'extraire les données de fichier ".osm" et de les stocker dans des objet "node" et "nodeGroup". 
    /// </summary>
    /// <param name="nameMap"> nom du fichier ".osm" dont on doit extraire les infos </param>
    public void readFileOSM(string nameMap)
    {
        counter = 0;
        buildingCounter = 0;
        string line;

        long id = 0;
        float lat = 0f;
        float lon = 0f;

        // Read the file and display it line by line.
        System.IO.StreamReader file = new System.IO.StreamReader(path + "Maps/" + nameMap + ".osm");

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

                main.nodes.Add(new Node(id, lat, lon));
                counter++;
            }

            // on recupere les batiments
            if (line.Contains("<way"))
            {
                // on créé un nouveau groupement de nodes
                NodeGroup current = new NodeGroup(long.Parse(line.Substring(line.IndexOf("id=") + 4, line.IndexOf("\" visible") - line.IndexOf("id=") - 4)));

                //lecture d'une nouvelle ligne
                line = file.ReadLine();

                // on remplit ce groupement de node
                while (!line.Contains("</way>"))
                {
                    // cas d'ajout d'un node
                    if (line.Contains("<nd"))
                    {
                        // on recupere l'id du node
                        long reference = long.Parse(line.Substring(line.IndexOf("ref=") + 5, line.IndexOf("\"/>") - line.IndexOf("ref=") - 5));

                        foreach (Node n in main.nodes)
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

                        //Ajout du nom au nodegroup courrent
                        current.setName(value);

                        // on ajoute le tag
                        current.addTag(key, value);
                        if (key.Equals("building") && value.Equals("yes"))
                        {
                            buildingCounter++;
                        }
                    }

                    line = file.ReadLine();
                }
                main.nodeGroups.Add(current);
            }

        }

        file.Close();
        //Debug.Log ("There were "+ counter +" nodes.");
    }

    ///</summary>
    /// Methode createResumeFile :
    /// Cree un fichier propre ou les informations sont resumees
    /// </summary>
    /// <param name="nameFile"> nom du fichier ou l'on inscrit les informations </param>
    public void createResumeFile(string nameFile)
    {
        // listing des balises de location "country,region,town,district"
        foreach(NodeGroup ngp in main.nodeGroups)
        {
            if (!cou.Contains(ngp.country))
            {
                cou.Add(ngp.country);
            }
            if (!reg.Contains(ngp.region))
            {
                reg.Add(ngp.region);
            }
            if (!tow.Contains(ngp.town))
            {
                tow.Add(ngp.town);
            }
            if (!dis.Contains(ngp.district))
            {
                dis.Add(ngp.district);
            }
            
        }
        
        string pathString = path + "MapsResumed/" + nameFile + "Resumed.osm";
        string buildingName;

        //on créé le fichier et on va écrire dedans
        System.IO.StreamWriter file = new System.IO.StreamWriter(pathString);
        file.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        file.WriteLine("<bounds minlat=\"" + minlat + "\" minlon=\"" + minlon + "\" maxlat=\"" + maxlat + "\" maxlon=\"" + maxlon + "\"/>");


        //Ecriture premiere balise earth
        file.WriteLine("\t<earth>");

       
        
        //ecriture des locations
        foreach(string str1 in cou)
        {
            file.WriteLine("\t\t<Country c=\"" + str1 + "\">");
            // a prevoir ici l'ecriture des infos

            foreach(string str2 in reg)
            {
                file.WriteLine("\t\t\t<Region r=\"" + str2 + "\">");
                // a prevoir ici l'ecriture des infos

                foreach(string str3 in tow)
                {
                    file.WriteLine("\t\t\t\t<Town t=\"" + str3 + "\">");
                    // a prevoir ici l'ecriture des infos

                    foreach(string str4 in dis)
                    {
                        // a prevoir ici l'ecriture des infos
                        file.WriteLine("\t\t\t\t\t<District d=\"" + str4 + "\">");
                        foreach (NodeGroup ngp in main.nodeGroups)
                        {
                            if ((ngp.country == str1) && (ngp.region == str2) && (ngp.town == str3) && (ngp.district == str4))
                            {
                                if (ngp.isBuilding())
                                {
                                    //on récupère le nom du batiment
                                    buildingName = ngp.getName();

                                    file.WriteLine("\t\t\t\t\t\t<building id=\"" + ngp.id + "\" name=\"" + buildingName + "\">");

                                    //ecriture des nodes
                                    foreach (Node n in ngp.nodes)
                                    {
                                        file.WriteLine("\t\t\t\t\t\t\t<node id=\"" + n.id + "\" lat=\"" + n.latitude + "\" lon=\"" + n.longitude + "\"/>");
                                    }
                                    //ecriture balise fin de building
                                    file.WriteLine("\t\t\t\t\t\t</building id=\"" + ngp.id + "\">");
                                }
                            }
                        }
                        file.WriteLine("\t\t\t\t\t</District>");
                    }
                    file.WriteLine("\t\t\t\t</Town>");
                }
                file.WriteLine("\t\t\t</Region>");
            }
            file.WriteLine("\t\t</Country>");
        }
                
        //Fermeture de le balise earth
        file.WriteLine("\t</earth>");
        //Fermeture de la balise xml
        file.WriteLine("</xml>");

        // Fermeture du fichier 
        file.Close();
    }

    /// <summary>
    /// Methode createSettingsFile :
    /// Cree un fichier de parametre
    /// </summary>
    /// <param name="nameFile"> nom du fichier ou l'on inscrit les informations </param>
    public void createSettingsFile(string nameFile)
    {
        string pathString = path + "MapsSettings/" + nameFile + "Settings.osm";
        string buildingName;

        //on créé le fichier et on va écrire dedans
        System.IO.StreamWriter file = new System.IO.StreamWriter(pathString);
        file.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");


        // on réécrit la liste des batiments en mettant des options par défaut
        file.WriteLine("\t<buildingList number=\"" + buildingCounter + "\" >");
        foreach (NodeGroup ngp in main.nodeGroups)
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

    /// <summary>
    /// Methode readSettingsFile :
    /// Lit un fichier setting
    /// </summary>
    /// <param name="nameFile"> nom du fichier setting a lire </param>
    public void readSettingsFile(string nameFile)
    {
        string pathString = path + "MapsSettings/" + nameFile + "Settings.osm";
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
                foreach (NodeGroup ngp in main.nodeGroups)
                {
                    if (ngp.equals(ng))
                    {
                        ngp.addTag("etages", etages.ToString());
                        //ngp.addTag("temperature", (int)Random.Range(0, 35) + "°C");
                        //ngp.addTag("humidité", (int)Random.Range(1, 10) + "%");
                    }
                }
            }
        }

        file.Close();
    }

    /// <summary>
    /// Metode readSettingFile :
    /// Lit un fichier resume
    /// </summary>
    /// <param name="nameFile"> nom du fichier resume a lire </param>
    public void readResumeFile(string nameFile)
    {
        counter = 0;
        buildingCounter = 0;
        string line;

        long id = 0;
        float lat = 0f;
        float lon = 0f;

        //Variables permettant de recuperer les balises
        string strCou="country";
        string strReg="region";
        string strTow="town";
        string strDis="district";

        // Read the file and display it line by line.
        System.IO.StreamReader file = new System.IO.StreamReader(path + "MapsResumed/" + nameFile + ".osm");
        while ((line = file.ReadLine()) != null)
        {
            // Recuperation de limites de la carte
            if (line.Contains("<bounds"))
            {
                minlat = float.Parse(line.Substring(line.IndexOf("minlat=") + 8, 9)) * 1000f;
                maxlat = float.Parse(line.Substring(line.IndexOf("maxlat=") + 8, 9)) * 1000f;
                minlon = float.Parse(line.Substring(line.IndexOf("minlon=") + 8, 9)) * 1000f;
                maxlon = float.Parse(line.Substring(line.IndexOf("maxlon=") + 8, 9)) * 1000f;
            }

            // Recuperation des balises
            if (line.Contains("<Country"))
            {
                strCou = line.Substring(line.IndexOf("c=") + 3, line.IndexOf("\">") - line.IndexOf("c=") - 3);
            }
            if (line.Contains("<Region"))
            {
                strReg =line.Substring(line.IndexOf("r=") + 3, line.IndexOf("\">") - line.IndexOf("r=") - 3);
            }
            if (line.Contains("<Town"))
            {
                strTow =line.Substring(line.IndexOf("t=") + 3, line.IndexOf("\">") - line.IndexOf("t=") - 3);
            }
            if (line.Contains("<District"))
            {
                strDis =line.Substring(line.IndexOf("d=") + 3, line.IndexOf("\">") - line.IndexOf("d=") - 3);
            }

            //Recuparation des batiments
            if (line.Contains("<building"))
            {

                // Creation d'un nouveau nodegroup
                NodeGroup current = new NodeGroup(long.Parse(line.Substring(line.IndexOf("id=") + 4, line.IndexOf("name=") - line.IndexOf("id=") - 6)));
                current.setName(line.Substring(line.IndexOf("name=") + 6, line.IndexOf("\" >") - line.IndexOf("name=") - 6));

                // Lecture d'une nouvelle ligne
                line = file.ReadLine();
                while (!line.Contains("</building"))
                {
                    if (line.Contains("<node"))
                    {
                        // Recuperation des nodes
                        id = long.Parse(line.Substring(line.IndexOf("id=") + 4, line.IndexOf("\" lat = ") - line.IndexOf("id=") - 4));
                        lat = float.Parse(line.Substring(line.IndexOf("lat=") + 5, 9));
                        lon = float.Parse(line.Substring(line.IndexOf("lon=") + 5, 9));
                        Node nde = new Node(id, lat, lon);
                        main.nodes.Add(nde);
                        counter++;

                        // Ajout du nouveau node au nodegroup
                        current.addNode(nde);
                        

                    }

                    // Ajout des balises de location dans le nodegroup
                    current.setCountry(strCou);
                    current.setRegion(strReg);
                    current.setTown(strTow);
                    current.setDistrict(strDis);

                    // Changement de ligne
                    line = file.ReadLine();
                }
                main.nodeGroups.Add(current);
            }
        }
    }

}
