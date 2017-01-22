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
                main.minlat = double.Parse(line.Substring(line.IndexOf("minlat=") + 8, line.IndexOf("\" minlon=") - line.IndexOf("minlat=") - 8));
                main.maxlat = double.Parse(line.Substring(line.IndexOf("maxlat=") + 8, line.IndexOf("\" maxlon=") - line.IndexOf("maxlat=") - 8));
                main.minlon = double.Parse(line.Substring(line.IndexOf("minlon=") + 8, line.IndexOf("\" maxlat=") - line.IndexOf("minlon=") - 8));
                main.maxlon = double.Parse(line.Substring(line.IndexOf("maxlon=") + 8, line.IndexOf("\"/>") - line.IndexOf("maxlon=") - 8));
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
                    
                    //Test pour trouver des caractéristiques de ce node
                    line = file.ReadLine();
                    if (line.Contains("<tag"))
                    {
                        string key = line.Substring(line.IndexOf("k=") + 3, line.IndexOf("\" v=") - line.IndexOf("k=") - 3);
                        string value = line.Substring(line.IndexOf("v=") + 3, line.IndexOf("\"/>") - line.IndexOf("v=") - 3);

                        // On regarde si c'est la key Natural qui contient les arbres
                        if (key.Equals("natural"))
                        {
                            NodeGroup current = new NodeGroup(id);

                            current.addTag(key, value);

                            if (current.isTree())
                            {
                                current.setName(value);
                            }
                            //On ajoute le node de cet arbre au NodeGroup de l'arbre.
                            current.addNode(new Node(id, lon, lat));

                            //On ajoute le NodeGroup à la liste des nodeGroup du Main.
                            main.nodeGroups.Add(current);
                        }

                        //On regarde si c'est la key highway qui contient les feux tricolores
                        if (key.Equals("highway"))
                        {
                            NodeGroup current = new NodeGroup(id);

                            current.addTag(key, value);

                            if (current.isFeuTri())
                            {
                                current.setName(value);
                            }
                            //On ajoute le node de ce feu tricolore au NodeGroup de ce feu tricolore.
                            current.addNode(new Node(id, lon, lat));

                            //On ajoute le NodeGroup à la liste des nodeGroup du Main.
                            main.nodeGroups.Add(current);
                        }
                    }
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
                NodeGroup current = new NodeGroup(double.Parse(line.Substring(line.IndexOf("id=") + 4, line.IndexOf("\" visible") - line.IndexOf("id=") - 4)));

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

                        //Si on a une route alors, on a peut être un nombre de voie et on la récupère
                        if (key.Equals("lanes"))
                        {
                            int value2 = int.Parse(line.Substring(line.IndexOf("v=") + 3, line.IndexOf("\"/>") - line.IndexOf("v=") - 3));
                            current.setNbVoie(value2);
                        }
                        //Si on a une vitesse maximum, on la récupère aussi
                        if (key.Equals("maxspeed"))
                        {
                            int value2 = int.Parse(line.Substring(line.IndexOf("v=") + 3, line.IndexOf("\"/>") - line.IndexOf("v=") - 3));
                            current.setVitMax(value2);
                        }

                        string value = line.Substring(line.IndexOf("v=") + 3, line.IndexOf("\"/>") - line.IndexOf("v=") - 3);

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
                        //Si la clé est le nom du batiment /de la route, on le rentre directement dans les valeurs du NodeGroup
                        if (key.Equals("name"))
                        {
                            current.setName(value);
                        }
                       
                    }

                    line = file.ReadLine();
                }
                if (current.isBuilding() || current.isHighway())
                {
                    main.nodeGroups.Add(current);
                }
                
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
        file.WriteLine("\t\t\t\t\t<Info lat=\"43.600000\" lon=\"1.433333\" dst=\"0.8\" nf=\"3\" roof=\"15\" type=\"pitched\"/>");
        //On crée les caractéristiques par défaut à l'UPS
        file.WriteLine("\t\t\t\t\t<district d=\"UPS\">");
        file.WriteLine("\t\t\t\t\t\t<Info lat=\"43.560397\" lon=\"1.468820\" dst=\"0.03\" nf=\"4\" roof=\"0\" type=\"flat\"/>");

        //Ici on crée les caractéristiques des différents buildings de l'UPS si nous les avons
        file.WriteLine("\t\t\t\t\t\t<building b=\"IRIT\">");
        file.WriteLine("\t\t\t\t\t\t\t<Info lat=\"43.561988\" lon=\"1.467984\" dst=\"0.0005\" nf=\"4\" roof=\"0\" type=\"flat\"/>");
        file.WriteLine("\t\t\t\t\t\t</building>");

        //Si on veut rajouter des caractéristiques pour un batiment précis de l'UPS, le faire ici
        file.WriteLine("\t\t\t\t\t</district>");

        //On crée les caractéristiques par défaut à l'UPS
        file.WriteLine("\t\t\t\t\t<district d=\"Centre-Ville\">");
        file.WriteLine("\t\t\t\t\t\t<Info lat=\"43.603236\" lon=\"1.444659\" dst=\"0.03\" nf=\"3\" roof=\"15\" type=\"pitched\"/>");

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
        string build = "";

        double lat = 0, longi = 0, distance = 0;

        int nb = 0;
        int roof = 0;
        string type = "";

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
                    if (Math.Sqrt(Math.Pow(lat - (ngp.getNode(0).getLatitude()), 2) + Math.Pow(longi - (ngp.getNode(0).getLongitude()), 2)) < distance)
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
                    if (Math.Sqrt(Math.Pow(lat - (ngp.getNode(0).getLatitude()), 2) + Math.Pow(longi - (ngp.getNode(0).getLongitude()), 2)) < distance)
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
                    if (Math.Sqrt(Math.Pow(lat - (ngp.getNode(0).getLatitude()), 2) + Math.Pow(longi - (ngp.getNode(0).getLongitude()), 2)) < distance)
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
                //On récupère le batiment de la ligne
                build = line.Substring(line.IndexOf("b=") + 3, line.IndexOf("\">") - line.IndexOf("b=") - 3);
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
                        ngp.setName(build);
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
    /// Methode createResumeFile :
    /// Cree un fichier propre ou les informations sont resumees
    /// </summary>
    /// <param name="nameFile"> nom du fichier ou l'on inscrit les informations </param>
    public void createResumeFile(string nameFile)
    {
        // listing des balises de location "country,region,town,district"
        foreach (NodeGroup ngp in main.nodeGroups)
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
        string highwayName;
        string typeRoof;
        int angleRoof;
        int nbFloor;
        double ID;
        string typeRoute;
        int nbVoie;
        int maxspeed;


        //on créé le fichier et on va écrire dedans
        System.IO.StreamWriter file = new System.IO.StreamWriter(pathString);
        file.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        file.WriteLine("<bounds minlat=\"" +main.minlat + "\" minlon=\"" + main.minlon + "\" maxlat=\"" + main.maxlat + "\" maxlon=\"" + main.maxlon + "\"/>");


        //Ecriture premiere balise earth
        file.WriteLine("\t<earth>");

        //ecriture des locations
        foreach (string str1 in cou)
        {
            file.WriteLine("\t\t<Country c=\"" + str1 + "\">");

            foreach (string str2 in reg)
            {
                file.WriteLine("\t\t\t<Region r=\"" + str2 + "\">");

                foreach (string str3 in tow)
                {
                    file.WriteLine("\t\t\t\t<Town t=\"" + str3 + "\">");

                    foreach (string str4 in dis)
                    {
                        
                        file.WriteLine("\t\t\t\t\t<District d=\"" + str4 + "\">");
                        foreach (NodeGroup ngp in main.nodeGroups)
                        {
                            if ((ngp.getCountry() == str1) && (ngp.getRegion() == str2) && (ngp.getTown() == str3) && (ngp.getDistrict() == str4))
                            {
                                //Si c'est un bâtiment 
                                if (ngp.isBuilding())
                                {
                                    //on récupère le nom du batiment et ses caractéristiques
                                    buildingName = ngp.getName();
                                    nbFloor = ngp.getNbFloors();
                                    typeRoof = ngp.getType();
                                    angleRoof = ngp.getAngle();
                                    ID = ngp.getID();

                                    //On écrit ces infos sur le ...Resumed
                                    file.WriteLine("\t\t\t\t\t\t<building>");
                                    file.WriteLine("\t\t\t\t\t\t\t<Info id=\"" + ID + "\" name=\"" + buildingName + "\" nbFloor=\"" + nbFloor + "\" type=\"" + typeRoof + "\" angle=\"" + angleRoof + "\"/>");

                                    //ecriture des nodes
                                    foreach (Node n in ngp.nodes)
                                    {
                                        file.WriteLine("\t\t\t\t\t\t\t<node id=\"" + n.getID() + "\" lat=\"" + n.getLatitude() + "\" lon=\"" + n.getLongitude() + "\"/>");
                                    }
                                    //ecriture balise fin de building
                                    file.WriteLine("\t\t\t\t\t\t</building>");
                                }

                                //Si c'est un arbre
                                if (ngp.isTree())
                                {
                                    //On récupère les caractéristiques de l'arbre
                                    ID = ngp.getID();

                                    //On écrit ces infos sur le ...Resumed
                                    file.WriteLine("\t\t\t\t\t\t<tree>");
                                    file.WriteLine("\t\t\t\t\t\t\t<Info id=\"" + ID + "\"/>");
                                    //ecriture des nodes
                                    foreach (Node n in ngp.nodes)
                                    {
                                        file.WriteLine("\t\t\t\t\t\t\t<node id=\"" + n.getID() + "\" lat=\"" + n.getLatitude() + "\" lon=\"" + n.getLongitude() + "\"/>");
                                    }
                                    //ecriture balise fin d'arbre
                                    file.WriteLine("\t\t\t\t\t\t</tree>");
                                }

                                //Si c'est un feu tricolore
                                if (ngp.isFeuTri())
                                {
                                    //On récupère les caractéristiques du feu tricolore
                                    ID = ngp.getID();

                                    //On écrit ces infos sur le ...Resumed
                                    file.WriteLine("\t\t\t\t\t\t<feuTri>");
                                    file.WriteLine("\t\t\t\t\t\t\t<Info id=\"" + ID + "\"/>");
                                    //ecriture des nodes
                                    foreach (Node n in ngp.nodes)
                                    {
                                        file.WriteLine("\t\t\t\t\t\t\t<node id=\"" + n.getID() + "\" lat=\"" + n.getLatitude() + "\" lon=\"" + n.getLongitude() + "\"/>");
                                    }
                                    //ecriture balise fin d'arbre
                                    file.WriteLine("\t\t\t\t\t\t</feuTri>");
                                }

                                // Si c'est une route
                                if (ngp.isHighway()){
                                    //on recupère les données sur la route
                                    ID = ngp.getID();
                                    typeRoute = ngp.GetTagValue("highway");
                                    highwayName = ngp.getName();
                                    nbVoie = ngp.getNbVoie();
                                    maxspeed = ngp.getVitMax();

                                    //On écrit ces infos sur le ...Resumed
                                    file.WriteLine("\t\t\t\t\t\t<highway>");
                                    file.WriteLine("\t\t\t\t\t\t\t<Info id=\"" + ID + "\" type=\"" + typeRoute + "\" name=\"" + highwayName + "\" nbVoie=\"" + nbVoie + "\" maxspd=\"" + maxspeed + "\"/>");

                                    //ecriture des nodes
                                    foreach (Node n in ngp.nodes)
                                    {
                                        file.WriteLine("\t\t\t\t\t\t\t<node id=\"" + n.getID() + "\" lat=\"" + n.getLatitude() + "\" lon=\"" + n.getLongitude() + "\"/>");
                                    }
                                    //ecriture balise fin de la route
                                    file.WriteLine("\t\t\t\t\t\t</highway>");
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
    /// Metode readSettingFile :
    /// Lit un fichier resume
    /// </summary>
    /// <param name="nameFile"> nom du fichier resume a lire </param>
    public void readResumeFile(string nameFile)
    {
        int counter = 0;
        int buildingCounter = 0;
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
                main.minlat = double.Parse(line.Substring(line.IndexOf("minlat=") + 8, line.IndexOf("\" minlon=") - line.IndexOf("minlat=") - 8));
                main.maxlat = double.Parse(line.Substring(line.IndexOf("maxlat=") + 8, line.IndexOf("\" maxlon=") - line.IndexOf("maxlat=") - 8));
                main.minlon = double.Parse(line.Substring(line.IndexOf("minlon=") + 8, line.IndexOf("\" maxlat=") - line.IndexOf("minlon=") - 8));
                main.maxlon = double.Parse(line.Substring(line.IndexOf("maxlon=") + 8, line.IndexOf("\"/>") - line.IndexOf("maxlon=") - 8));
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
                line = file.ReadLine();

                // Creation d'un nouveau nodegroup
                NodeGroup current = new NodeGroup(double.Parse(line.Substring(line.IndexOf("id=") + 4, line.IndexOf("name=") - line.IndexOf("id=") - 6)));

                // Ajout des caractéristiques du batiment
                current.setName(line.Substring(line.IndexOf("name=") + 6, line.IndexOf("\" nbFloor") - line.IndexOf("name=") - 6));
                current.setNbFloors(int.Parse(line.Substring(line.IndexOf("nbFloor=") + 9, line.IndexOf("\" type") - line.IndexOf("nbFloor=") - 9)));
                current.setType(line.Substring(line.IndexOf("type=") + 6, line.IndexOf("\" angle") - line.IndexOf("type=") - 6));
                current.setAngle(int.Parse(line.Substring(line.IndexOf("angle=") + 7, line.IndexOf("\"/>") - line.IndexOf("angle=") - 7)));

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

            // Récupération des routes
            if (line.Contains("<highway"))
            {
                line = file.ReadLine();

                // Creation d'un nouveau nodegroup
                NodeGroup current = new NodeGroup(double.Parse(line.Substring(line.IndexOf("id=") + 4, line.IndexOf("type=") - line.IndexOf("id=") - 6)));

                // Ajout des caractéristiques de la route
                current.addTag("highway", line.Substring(line.IndexOf("type=") + 6, line.IndexOf("\" name") - line.IndexOf("type=") - 6));
                current.setName(line.Substring(line.IndexOf("name=") + 6, line.IndexOf("\" nbVoie") - line.IndexOf("name=")-6));
                current.setNbVoie(int.Parse(line.Substring(line.IndexOf("nbVoie=") + 8, line.IndexOf("\" maxspd") - line.IndexOf("nbVoie=") - 8)));
                current.setVitMax(int.Parse(line.Substring(line.IndexOf("maxspd=") + 8, line.IndexOf("\"/>") - line.IndexOf("maxspd=") - 8)));

                // Lecture d'une nouvelle ligne
                line = file.ReadLine();

                while (!line.Contains("</highway"))
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

                //Ajout du nodeGroup courent à la liste main.nodeGroups
                main.nodeGroups.Add(current);
            }

            //Recuparation des arbres
            if (line.Contains("<tree"))
            {
                line = file.ReadLine();

                // Creation d'un nouveau nodegroup
                NodeGroup current = new NodeGroup(double.Parse(line.Substring(line.IndexOf("id=") + 4, line.IndexOf("name=") - line.IndexOf("id=") - 6)));

                // Lecture d'une nouvelle ligne
                line = file.ReadLine();
                while (!line.Contains("</tree"))
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
                current.addTag("natural", "tree");

                //Ajout du nodeGroup courent à la liste main.nodeGroups
                main.nodeGroups.Add(current);
            }


            //Recuparation des feux tricolores
            if (line.Contains("<feuTri"))
            {
                line = file.ReadLine();

                // Creation d'un nouveau nodegroup
                NodeGroup current = new NodeGroup(double.Parse(line.Substring(line.IndexOf("id=") + 4, line.IndexOf("name=") - line.IndexOf("id=") - 6)));

                // Lecture d'une nouvelle ligne
                line = file.ReadLine();
                while (!line.Contains("</feuTri"))
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
                current.addTag("highway", "traffic_signals");

                //Ajout du nodeGroup courent à la liste main.nodeGroups
                main.nodeGroups.Add(current);
            }

        }
        Debug.Log("There are " + counter + " nodes.");
        Debug.Log("There are " + buildingCounter + " buildings.");
        Debug.Log("There are " + main.nodeGroups.Count + " nodegroups.");
    }

}
