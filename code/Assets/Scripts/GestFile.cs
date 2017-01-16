using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GestFile
{

    // compteurs de nodes et groupes de nodes (batiments puis routes) respectivement
    protected int counter;
    protected int buildingCounter;
    protected int highwayCounter;
	// coordonnées min et max de la carte
    private double minlat, maxlat, minlon, maxlon;

    // chemin d'acces et nom du fichier par defaut
    protected string path = @"./Assets/";
    protected string fileName;

    // Arraylist permettant de stocker les balises
    protected ArrayList cou = new ArrayList();
    protected ArrayList reg = new ArrayList();
    protected ArrayList tow = new ArrayList();
    protected ArrayList dis = new ArrayList();


    //Constructeur par deffaut
    public GestFile()
    {   
    }

    //Constructeur 
    public GestFile(string path, string fileName)
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
		int cpt1 = 0, cpt2 = 0, cpt3 = 0, cpt4 = 0, cpt5 = 0, cpt6 = 0, cpt7 = 0;
        counter = 0;
        buildingCounter = 0;
		highwayCounter = 0;
        string line;

        double id = 0;
        double lat = 0d;
        double lon = 0d;

        // Read the file and display it line by line.
        System.IO.StreamReader file = new System.IO.StreamReader(path + "Maps/" + nameMap + ".osm");

        // on commence par repertorier toutes les nodes de la  carte
        while ((line = file.ReadLine()) != null)
        {
            // on recupère les extremites
            if (line.Contains("<bounds"))
            {
                minlat = double.Parse(line.Substring(line.IndexOf("minlat=") + 8, line.IndexOf("\" minlon=") - line.IndexOf("minlat=") - 8));
                maxlat = double.Parse(line.Substring(line.IndexOf("maxlat=") + 8, line.IndexOf("\" maxlon=") - line.IndexOf("maxlat=") - 8));
                minlon = double.Parse(line.Substring(line.IndexOf("minlon=") + 8, line.IndexOf("\" maxlat=") - line.IndexOf("minlon=") - 8));
                maxlon = double.Parse(line.Substring(line.IndexOf("maxlon=") + 8, line.IndexOf("\"/>") - line.IndexOf("maxlon=") - 8));
            

                main.minlat = minlat;
				main.maxlat = maxlat;
				main.minlon = minlon;
				main.maxlon = maxlon;
            }
				
            // on recupère les nodes
            if (line.Contains("<node"))
            {
                id = double.Parse(line.Substring(line.IndexOf("id=") + 4, line.IndexOf("\" visible") - line.IndexOf("id=") - 4));
                lat = double.Parse(line.Substring(line.IndexOf("lat=") + 5, line.IndexOf("\" lon=") - line.IndexOf("lat=") - 5));

                //le dernier node sur osm a une baliste defin "> au lieux de "/> 
                if (line.Contains("\"/>"))
                {
                    lon = double.Parse(line.Substring(line.IndexOf("lon=") + 5, line.IndexOf("\"/>") - line.IndexOf("lon=") - 5));

                }
                else
                {
                    lon = double.Parse(line.Substring(line.IndexOf("lon=") + 5, line.IndexOf("\">") - line.IndexOf("lon=") - 5));
                }

                // création d'un point 
                main.nodes.Add(new Node(id, lon, lat));

                //incrément du compteur de point
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
                        double reference = double.Parse(line.Substring(line.IndexOf("ref=") + 5, line.IndexOf("\"/>") - line.IndexOf("ref=") - 5));

                        foreach (Node n in main.nodes)
                        {
                            // on ajoute le node a la liste de ceux qui compose le node groupe
                            if (n.getID() == reference)
                            {
                                current.addNode(n);
                            }
                        }
                    }

                    if (line.Contains("<tag"))
                    {
                        string key = line.Substring(line.IndexOf("k=") + 3, line.IndexOf("\" v=") - line.IndexOf("k=") - 3);
                        string value = line.Substring(line.IndexOf("v=") + 3, line.IndexOf("\"/>") - line.IndexOf("v=") - 3);

                        //Ajout du nom au nodegroup courant
                        current.setName(value);

                        // on ajoute le tag
                        current.addTag(key, value);
                        if (key.Equals("building") && value.Equals("yes"))
                        {
                            buildingCounter++;
                        }
						if (key.Equals("highway") && (value.Equals("primary") || value.Equals("secondary") || value.Equals("tertiary") 
							|| value.Equals("unclassified") || value.Equals("residential") || value.Equals("service")))// || value.Equals("footway")) )
						{
							highwayCounter++;
						}
						if (key.Equals("highway") && (value.Equals("primary")  || value.Equals("tertiary") 
							|| value.Equals("unclassified") || value.Equals("residential") || value.Equals("service")))// || value.Equals("footway")) )
							cpt1++;
						if (key.Equals ("highway") && value.Equals ("secondary"))
							cpt2++;
						if (key.Equals ("highway") && value.Equals ("tertiary"))
							cpt3++;
						if (key.Equals ("highway") && value.Equals ("unclassified"))
							cpt4++;
						if (key.Equals ("highway") && value.Equals ("residential"))
							cpt5++;
						if (key.Equals ("highway") && value.Equals ("service"))
							cpt6++;
						if (key.Equals ("highway") && value.Equals ("footway"))
							cpt7++;
                        //Si la clé est un type de toit, on le rentre directement dans les valeurs du NodeGroup
                        if (key.Equals("roof:shape"))
                        {
                            current.setType(value);
                        }
                    }



                    line = file.ReadLine();
                }
                main.nodeGroups.Add(current);
            }

        }

        file.Close();
//      Debug.Log ("There are "+ counter +" nodes.");
//		Debug.Log ("There are "+ main.nodeGroups.Count +" ways.");
//		Debug.Log ("There are "+ buildingCounter +" buildings.");
//		Debug.Log ("There are "+ highwayCounter +" highways.");
//		Debug.Log (cpt1 + " primary.");
//		Debug.Log (cpt2 + " secondary.");
//		Debug.Log (cpt3 + " tertiary.");
//		Debug.Log (cpt4 + " unclassified.");
//		Debug.Log (cpt5 + " residential.");
//		Debug.Log (cpt6 + " service.");
//		Debug.Log (cpt7 + " footway.");

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
            if (!cou.Contains(ngp.getCountry()))
            {
                cou.Add(ngp.getCountry());
            }
            if (!reg.Contains(ngp.getRegion()))
            {
                reg.Add(ngp.getRegion());
            }
            if (!tow.Contains(ngp.getTown()))
            {
                tow.Add(ngp.getTown());
            }
            if (!dis.Contains(ngp.getDistrict()))
            {
                dis.Add(ngp.getDistrict());
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
                            if ((ngp.getCountry() == str1) && (ngp.getRegion() == str2) && (ngp.getTown() == str3) && (ngp.getDistrict() == str4))
                            {
                                if (ngp.isBuilding())
                                {
                                    //on récupère le nom du batiment
                                    buildingName = ngp.getName();

                                    file.WriteLine("\t\t\t\t\t\t<building id=\"" + ngp.getID() + "\" name=\"" + buildingName + "\">");

                                    //ecriture des nodes
                                    foreach (Node n in ngp.nodes)
                                    {
                                        file.WriteLine("\t\t\t\t\t\t\t<node id=\"" + n.getID() + "\" lat=\"" + n.getLatitude() + "\" lon=\"" + n.getLongitude() + "\"/>");
                                    }
                                    //ecriture balise fin de building
                                    file.WriteLine("\t\t\t\t\t\t</building id=\"" + ngp.getID() + "\">");
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
    /// Methode readSettingsFile :
    /// Lit un fichier setting
    /// </summary>
    /// <param name="nameFile"> nom du fichier setting a lire </param>
    public void readSettingsFile(string nameFile)
    {
        string pathString = path + "MapsSettings/" + nameFile + "Settings.osm";
        string line;

        string country = "";
        string region = "";
        string town = "";
        string district = "";

        double lat = 0, longi = 0, distance = 0;

        int nb = 0;
        int roof = 0;
        string type = "";
        double id = 0;

        string name = "";

        //on lit le fichier de configuration
        System.IO.StreamReader file = new System.IO.StreamReader(pathString);

        while ((line = file.ReadLine()) != null)
        {
            if (line.Contains("<earth "))
            {
                line = file.ReadLine();
                if (line.Contains("<Info "))
                {
                    nb = int.Parse(line.Substring(line.IndexOf("nf=") + 4, line.IndexOf("\" roof") - line.IndexOf("nf=") - 4));
                    roof = int.Parse(line.Substring(line.IndexOf("roof=") + 6, line.IndexOf("\" type") - line.IndexOf("roof=") - 6));
                    type = line.Substring(line.IndexOf("type=") + 6, line.IndexOf("\"/>") - line.IndexOf("type=") - 6);
                }
                //On donne aux nodegroup les attributs par défaut de la planète Terre
                foreach (NodeGroup ngp in main.nodeGroups)
                {
                    ngp.setNbFloors(nb);
                    ngp.setAngle(roof);
                    ngp.setType(type);
                }
            }

            //PAYS
            if (line.Contains("<country "))
            {
                //On récupère le pays de la ligne
                country = line.Substring(line.IndexOf("c=") + 3, line.IndexOf("\">") - line.IndexOf("c=") - 3);
                //On récupère les paramètres longitude, latitude du centre du pays et distance au centre du pays
                line = file.ReadLine();
                if (line.Contains("<Info "))
                {
                    lat = double.Parse(line.Substring(line.IndexOf("lat=") + 5, line.IndexOf("\" lon") - line.IndexOf("lat=") - 5));
                    longi = double.Parse(line.Substring(line.IndexOf("lon=") + 5, line.IndexOf("\" dst") - line.IndexOf("lon=") - 5));
                    distance = double.Parse(line.Substring(line.IndexOf("dst=") + 5, line.IndexOf("\" nf") - line.IndexOf("dst=") - 5));

                    nb = int.Parse(line.Substring(line.IndexOf("nf=") + 4, line.IndexOf("\" roof") - line.IndexOf("nf=") - 4));
                    roof = int.Parse(line.Substring(line.IndexOf("roof=") + 6, line.IndexOf("\" type") - line.IndexOf("roof=") - 6));
                    type = line.Substring(line.IndexOf("type=") + 6, line.IndexOf("\"/>") - line.IndexOf("type=") - 6);
                }
                //Ici on regarde si la distance entre les coos d'un des points du nodegroup et le centre du pays est < distance
                //Si c'est le cas, alors ce nodegroup appartient au pays (on peut lui mettre country comme attribut de ngp.country
                foreach (NodeGroup ngp in main.nodeGroups)
                {
                    if (Math.Sqrt(Math.Pow(lat - (ngp.getNode(0).getLatitude()), 2) + Math.Pow(longi - (ngp.getNode(0).getLongitude()), 2)) < distance)
                    {
                        ngp.setCountry(country);
                        ngp.setNbFloors(nb);
                        ngp.setAngle(roof);
                        ngp.setType(type);

                    }
                }  
            }

            //REGIONS
            if (line.Contains("<region "))
            {
                //On récupère la region de la ligne
                region = line.Substring(line.IndexOf("r=") + 3, line.IndexOf("\">") - line.IndexOf("r=") - 3);
                //On récupère les paramètres longitude, latitude du centre du pays et distance au centre du pays
                line = file.ReadLine();
                if (line.Contains("<Info "))
                {
                    lat = double.Parse(line.Substring(line.IndexOf("lat=") + 5, line.IndexOf("\" lon") - line.IndexOf("lat=") - 5));
                    longi = double.Parse(line.Substring(line.IndexOf("lon=") + 5, line.IndexOf("\" dst") - line.IndexOf("lon=") - 5));
                    distance = double.Parse(line.Substring(line.IndexOf("dst=") + 5, line.IndexOf("\" nf") - line.IndexOf("dst=") - 5));

                    nb = int.Parse(line.Substring(line.IndexOf("nf=") + 4, line.IndexOf("\" roof") - line.IndexOf("nf=") - 4));
                    roof = int.Parse(line.Substring(line.IndexOf("roof=") + 6, line.IndexOf("\" type") - line.IndexOf("roof=") - 6));
                    type = line.Substring(line.IndexOf("type=") + 6, line.IndexOf("\"/>") - line.IndexOf("type=") - 6);
                }
                //Ici on regarde si la distance entre les coos d'un des points du nodegroup et le centre du pays est < distance
                //Si c'est le cas, alors ce nodegroup appartient au pays (on peut lui mettre country comme attribut de ngp.country
                foreach (NodeGroup ngp in main.nodeGroups)
                {
                    if (Math.Sqrt(Math.Pow(lat - (ngp.getNode(0).getLatitude() ), 2) + Math.Pow(longi - (ngp.getNode(0).getLongitude() ), 2)) < distance)
                    {
                        ngp.setRegion(region);
                        ngp.setNbFloors(nb);
                        ngp.setAngle(roof);
                        ngp.setType(type);
                    }
                }
            }

            //VILLES
            if (line.Contains("<town "))
            {
                //On récupère la ville de la ligne
                town = line.Substring(line.IndexOf("t=") + 3, line.IndexOf("\">") - line.IndexOf("t=") - 3);
                //On récupère les paramètres longitude, latitude du centre du pays et distance au centre du pays
                line = file.ReadLine();
                if (line.Contains("<Info "))
                {
                    lat = double.Parse(line.Substring(line.IndexOf("lat=") + 5, line.IndexOf("\" lon") - line.IndexOf("lat=") - 5));
                    longi = double.Parse(line.Substring(line.IndexOf("lon=") + 5, line.IndexOf("\" dst") - line.IndexOf("lon=") - 5));
                    distance = double.Parse(line.Substring(line.IndexOf("dst=") + 5, line.IndexOf("\" nf") - line.IndexOf("dst=") - 5));

                    nb = int.Parse(line.Substring(line.IndexOf("nf=") + 4, line.IndexOf("\" roof") - line.IndexOf("nf=") - 4));
                    roof = int.Parse(line.Substring(line.IndexOf("roof=") + 6, line.IndexOf("\" type") - line.IndexOf("roof=") - 6));
                    type = line.Substring(line.IndexOf("type=") + 6, line.IndexOf("\"/>") - line.IndexOf("type=") - 6);
                }
                //Ici on regarde si la distance entre les coos d'un des points du nodegroup et le centre du pays est < distance
                //Si c'est le cas, alors ce nodegroup appartient au pays (on peut lui mettre country comme attribut de ngp.country
                foreach (NodeGroup ngp in main.nodeGroups)
                {
                    if (Math.Sqrt(Math.Pow(lat - (ngp.getNode(0).getLatitude() ), 2) + Math.Pow(longi - (ngp.getNode(0).getLongitude() ), 2)) < distance)
                    {
                        ngp.setTown(town);
                        ngp.setNbFloors(nb);
                        ngp.setAngle(roof);
                        ngp.setType(type);
                    }
                }
            }

            //QUARTIERS
            if (line.Contains("<district "))
            {
                //On récupère le quartier de la ligne
                district = line.Substring(line.IndexOf("d=") + 3, line.IndexOf("\">") - line.IndexOf("d=") - 3);
                //On récupère les paramètres longitude, latitude du centre du pays et distance au centre du pays
                line = file.ReadLine();
                if (line.Contains("<Info "))
                {
                    lat = double.Parse(line.Substring(line.IndexOf("lat=") + 5, line.IndexOf("\" lon") - line.IndexOf("lat=") - 5));
                    longi = double.Parse(line.Substring(line.IndexOf("lon=") + 5, line.IndexOf("\" dst") - line.IndexOf("lon=") - 5));
                    distance = double.Parse(line.Substring(line.IndexOf("dst=") + 5, line.IndexOf("\" nf") - line.IndexOf("dst=") - 5));

                    nb = int.Parse(line.Substring(line.IndexOf("nf=") + 4, line.IndexOf("\" roof") - line.IndexOf("nf=") - 4));
                    roof = int.Parse(line.Substring(line.IndexOf("roof=") + 6, line.IndexOf("\" type") - line.IndexOf("roof=") - 6));
                    type = line.Substring(line.IndexOf("type=") + 6, line.IndexOf("\"/>") - line.IndexOf("type=") - 6);
                }
                //Ici on regarde si la distance entre les coos d'un des points du nodegroup et le centre du pays est < distance
                //Si c'est le cas, alors ce nodegroup appartient au pays (on peut lui mettre country comme attribut de ngp.country
                foreach (NodeGroup ngp in main.nodeGroups)
                {
                    if (Math.Sqrt(Math.Pow(lat - (ngp.getNode(0).getLatitude() ), 2) + Math.Pow(longi - (ngp.getNode(0).getLongitude() ), 2)) < distance)
                    {
                        ngp.setDistrict(district);
                        ngp.setNbFloors(nb);
                        ngp.setAngle(roof);
                        ngp.setType(type);
                    }
                }
            }

            //BATIMENTS SPECIFIQUES
            if (line.Contains("<building "))
            {
                //On récupère le pays de la ligne
                id = double.Parse(line.Substring(line.IndexOf("id=") + 4, line.IndexOf("\">") - line.IndexOf("id=") - 4));
                //On récupère les paramètres longitude, latitude du centre du pays et distance au centre du pays
                line = file.ReadLine();
                if (line.Contains("<Info "))
                {
                    name = line.Substring(line.IndexOf("name=") + 6, line.IndexOf("\" nf") - line.IndexOf("name=") - 6);
                    nb = int.Parse(line.Substring(line.IndexOf("nf=") + 4, line.IndexOf("\" roof") - line.IndexOf("nf=") - 4));
                    roof = int.Parse(line.Substring(line.IndexOf("roof=") + 6, line.IndexOf("\" type") - line.IndexOf("roof=") - 6));
                    type = line.Substring(line.IndexOf("type=") + 6, line.IndexOf("\"/>") - line.IndexOf("type=") - 6);
                }
                //Ici on regarde si la distance entre les coos d'un des points du nodegroup et le centre du pays est < distance
                //Si c'est le cas, alors ce nodegroup appartient au pays (on peut lui mettre country comme attribut de ngp.country
                foreach (NodeGroup ngp in main.nodeGroups)
                {
                    if (ngp.getID() == id)
                    {
                        ngp.setName(name);
                        ngp.setNbFloors(nb);
                        ngp.setAngle(roof);
                        ngp.setType(type);
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

        double id = 0d;
        double lat = 0d;
        double lon = 0d;

        //Variables permettant de recuperer les balises
        string strCou="country";
        string strReg="region";
        string strTow="town";
        string strDis="district";

        // Read the file and display it line by line.
        System.IO.StreamReader file = new System.IO.StreamReader(path + "MapsResumed/" + nameFile + "Resumed.osm");
        while ((line = file.ReadLine()) != null)
        {
            // Recuperation de limites de la carte
            if (line.Contains("<bounds"))
            {
                minlat = double.Parse(line.Substring(line.IndexOf("minlat=") + 8, line.IndexOf("\" minlon=") - line.IndexOf("minlat=") - 8));
                maxlat = double.Parse(line.Substring(line.IndexOf("maxlat=") + 8, line.IndexOf("\" maxlon=") - line.IndexOf("maxlat=") - 8));
                minlon = double.Parse(line.Substring(line.IndexOf("minlon=") + 8, line.IndexOf("\" maxlat=") - line.IndexOf("minlon=") - 8));
                maxlon = double.Parse(line.Substring(line.IndexOf("maxlon=") + 8, line.IndexOf("\"/>") - line.IndexOf("maxlon=") - 8));

                main.minlat = minlat;
                main.maxlat = maxlat;
                main.minlon = minlon;
                main.maxlon = maxlon;
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
                current.setName(line.Substring(line.IndexOf("name=") + 6, line.IndexOf("\">") - line.IndexOf("name=") - 6));

                // Lecture d'une nouvelle ligne
                line = file.ReadLine();
                while (!line.Contains("</building"))
                {
                    if (line.Contains("<node"))
                    {
                        // Recuperation des nodes
                        id = double.Parse(line.Substring(line.IndexOf("id=") + 4, line.IndexOf("\" lat=") - line.IndexOf("id=") - 4));
                        lat = double.Parse(line.Substring(line.IndexOf("lat=") + 5, line.IndexOf("\" lon=") - line.IndexOf("lat=") - 5));
                        lon = double.Parse(line.Substring(line.IndexOf("lon=") + 5, line.IndexOf("\"/>") - line.IndexOf("lon=") - 5));
                        
                        main.nodes.Add(new Node(id, lon, lat));
                        counter++;

                        // Ajout du nouveau node au nodegroup
                        current.addNode(new Node(id, lon, lat));
                        
                    }

                    // Changement de ligne
                    line = file.ReadLine();
                }
                // Ajout des balises de location dans le nodegroup
                current.setCountry(strCou);
                current.setRegion(strReg);
                current.setTown(strTow);
                current.setDistrict(strDis);

                //Ajout du tag 
                current.addTag("building", "yes");

                //Ajout du nodeGroup courent à la liste main.nodeGroups
                main.nodeGroups.Add(current);

                //Increment du compteur de batiment 
                buildingCounter++;
            }
        }
        Debug.Log("There are " + counter + " nodes.");
        Debug.Log("There are " + buildingCounter + " buildings.");
        Debug.Log("There are " + main.nodeGroups.Count + " buildings.");
    }

    /// <summary>
    /// Metode createSettingFile :
    /// Crée le fichier Setting contenant les paramètres de construction du mapResume
    /// </summary>
    /// <param name="nameFile"> nom du fichier resume a lire </param>
 
    public void createSettingsFile(string fileName)
    {

        string pathString = path + "MapsSettings/" + fileName + "Settings.osm";

        //on créé le fichier et on va écrire dedans
        System.IO.StreamWriter file = new System.IO.StreamWriter(pathString);
        file.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        //On crée les caractéristiques par défaut dans le monde
        file.WriteLine("\t<earth>");
        file.WriteLine("\t\t<Info nf=\"1\" roof=\"15\" type=\"pitched\"/>");
        //On crée les caractéristiques par défaut en France
        file.WriteLine("\t\t<country c=\"France\">");
        file.WriteLine("\t\t\t<Info lat=\"47.3833300\" lon=\"0.6833300\" dst=\"5\" nf=\"1\" roof=\"15\" type=\"pitched\"/>");
        //On crée les caractéristiques par défaut en Midi-Pyrenees
        file.WriteLine("\t\t\t<region r=\"Midi-Pyrenees\">");
        file.WriteLine("\t\t\t\t<Info lat=\"43.600000\" lon=\"1.433333\" dst=\"1.1\" nf=\"1\" roof=\"15\" type=\"pitched\"/>");
        //On crée les caractéristiques par défaut à Toulouse
        file.WriteLine("\t\t\t\t<town t=\"Toulouse\">");
        file.WriteLine("\t\t\t\t\t<Info lat=\"43.600000\" lon=\"1.433333\" dst=\"0.8\" nf=\"1\" roof=\"15\" type=\"pitched\"/>");
        //On crée les caractéristiques par défaut à l'UPS
        file.WriteLine("\t\t\t\t\t<district d=\"UPS\">");
        file.WriteLine("\t\t\t\t\t\t<Info lat=\"43.560397\" lon=\"1.468820\" dst=\"0.02\" nf=\"4\" roof=\"0\" type=\"flat\"/>");

        //Ici on crée les caractéristiques des différents buildings de l'UPS si nous les avons
        file.WriteLine("\t\t\t\t\t\t<building id=\"23905163\">");
        file.WriteLine("\t\t\t\t\t\t\t<Info name=\"IRIT\" nf=\"4\" roof=\"0\" type=\"flat\"/>");
        file.WriteLine("\t\t\t\t\t\t</building>");
        file.WriteLine("\t\t\t\t\t\t<building id=\"23905283\">");
        file.WriteLine("\t\t\t\t\t\t\t<Info name=\"3 PN\" nf=\"4\" roof=\"0\" type=\"flat\"/>");
        file.WriteLine("\t\t\t\t\t\t</building>");
        file.WriteLine("\t\t\t\t\t\t<building id=\"23905315\">");
        file.WriteLine("\t\t\t\t\t\t\t<Info name=\"LAPLACE (3R2)\" nf=\"4\" roof=\"0\" type=\"flat\"/>");
        file.WriteLine("\t\t\t\t\t\t</building>");

        //Si on veut rajouter des caractéristiques pour un batiment précis de l'UPS, le faire ici
        file.WriteLine("\t\t\t\t\t</district>");

        //On crée les caractéristiques par défaut à l'UPS
        file.WriteLine("\t\t\t\t\t<district d=\"Centre-Ville\">");
        file.WriteLine("\t\t\t\t\t\t<Info lat=\"43.603236\" lon=\"1.444659\" dst=\"0.02\" nf=\"1\" roof=\"15\" type=\"pitched\"/>");

        file.WriteLine("\t\t\t\t\t</district>");
        //Si on veut rajouter un quartier de la ville de Toulouse, le faire ici


        file.WriteLine("\t\t\t\t</town>");
        //Si on veut rajouter une ville de la région Midi-Pyrenees, le faire ici

        file.WriteLine("\t\t\t</region>");
        //Si on veut rajouter une région de France, le faire ici

        file.WriteLine("\t\t</country>");
        //Si on veut rajouter des pays, les ajouter à partir d'ici

        file.WriteLine("\t</earth>");
        //SI on veut changer de planète, pourquoi pas le faire ici :D
        file.WriteLine("</xml>");

        file.Close();
    }

}
