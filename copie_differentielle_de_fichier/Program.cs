/*
 * Alexandre Robert
 * 01/12/2022
 * 
 * Version 2.1.3 - Upgraded log with execution time
 * 
 * COPIE DE DOSSIERS ET FICHIERS AVEC CONFIGURATION
 * 
 * Utilise un document App.config.xml pour le repertoire source unique (le plus haut) depuis lequel copier et sa destination
 * Utilise un document folder.ini qui référencie le détail des répertoires (plus bas) à copier
 * 
 */

using System.Xml;
// Import du fichier App.Config.xml (spécifier son chemin)
XmlDocument AppConfig = new XmlDocument();
AppConfig.Load("K:\\08_Informatique\\15_Scripts\\Sauvegarde\\copie_differentielle_de_fichier\\App.Config.xml");
// Définition des valeurs selon le fichier
XmlNodeList source = AppConfig.GetElementsByTagName("source");
XmlNodeList destination = AppConfig.GetElementsByTagName("destination");
string sourceValue = source[0].InnerText;
string destinationValue = destination[0].InnerText;

// Liste des répertoires à copier à partir du fichier folder.ini (spécifier son chemin)
string[] folders = File.ReadAllLines("K:\\08_Informatique\\15_Scripts\\Sauvegarde\\copie_differentielle_de_fichier\\folder.ini");

//declaration des variables globales
int nbFichiersModifier = 0;
int nbFichierIgnore = 0;
int nbErreur = 0;
List<string> listeErreur = new List<string>();
string log = "K:\\08_Informatique\\15_Scripts\\Sauvegarde\\copie_differentielle_de_fichier\\logs\\" + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
const string SEPARATION = "--------------------------------------------------";


//************* PARAMETRAGE UTILISATEUR *************//

//affichage infos
Console.WriteLine("Informations définies dans le fichier App.Config.xml : \n");
Console.WriteLine("SOURCE : " + sourceValue);
Console.WriteLine("DESTINATION : " + destinationValue + "\n" + SEPARATION + "\n");
Console.WriteLine("La copie concerne l'entièreté des fichiers contenus dans les répertoires suivants :\n");
foreach (string folder in folders)
{
    Console.WriteLine(folder);
}

// Choix de la méthode de vérification des fichier avant copie
VerifDesFichiers();

// Choix de la méthode de copie
Console.WriteLine("\nChoix de la méthode de copie :");
Console.WriteLine("1 - Copie simple");
Console.WriteLine("2 - Copie différentielle");
int choixMethode = Convert.ToInt32(Console.ReadLine());
File.AppendAllText(log, "****************************************************************" + Environment.NewLine);

// Effectue la copie selon le choix (simple/différentielle)
if (choixMethode == 1)
{
    FaireCopyDirectory();
}
else if (choixMethode == 2)
{
    FaireCopyDirectoryDiff();
}
else
{
    Console.WriteLine("Choix incorrect");
}

//****************** METHODES ******************//

// Affichage erreurs
void AfficherErreur(int nbErreur, string log)
{
    if (nbErreur > 0)
    {
        File.AppendAllText(log, "Nombre d'erreur constatées : " + nbErreur + Environment.NewLine);
        foreach (string erreur in listeErreur)
        {
            File.AppendAllText(log, erreur + Environment.NewLine);
        }
    }
    else
    {
        File.AppendAllText(log, "Aucune erreur constatée" + Environment.NewLine);
    }
}

// Répète la méthode de copie simple pour chaque répertoire
void FaireCopyDirectory()
{
    Console.WriteLine("Vous vous apprêter à faire un copie simple des répertoires susmentionnés dans le répertoire suivant : \n" + destinationValue + "\n");
    AppuyezPourContinuer();
    DateTime dateDebut = DateTime.Now;
    File.AppendAllText(log, "Début de la copie simple :" + DateTime.Now + Environment.NewLine);
    
    //pour chaque répertoire
    foreach (string folder in folders)
    {
        try
        {
            //afficher le répertoire à copier
            Console.WriteLine(folder);
            //copie le répertoire et ses sous-répertoires
            CopyDirectory(sourceValue + folder, destinationValue + folder);
        }
        //erreur si l'accès au dossier n'est pas autorisé
        catch (Exception e)
        {
            Console.WriteLine("Erreur : " + e.Message);
            //ajout de cette erreur dans le fichier log et dans la liste d'erreur
            string time = DateTime.Now.ToString();
            nbErreur++;
            listeErreur.Add(e.Message + " " + time);
        }
    }

    AfficherErreur(nbErreur, log);
    Console.WriteLine("\nTache terminee \n\nInformations dans le fichier log : \n" + log);

    //ajout des infos dans le fichier log
    File.AppendAllText(log, "Fin: " + DateTime.Now + Environment.NewLine);
    File.AppendAllText(log, SEPARATION + Environment.NewLine);
    File.AppendAllText(log, "Liste des répertoires concernés : " + Environment.NewLine);
    foreach (string folder in folders) { File.AppendAllText(log, sourceValue + folder + Environment.NewLine); }
    File.AppendAllText(log, SEPARATION + Environment.NewLine);
    File.AppendAllText(log, "Copie simple terminée : \nNombre de fichiers copiés : " 
        + nbFichiersModifier + Environment.NewLine + "\nTemps d'éxecution : " 
        + (DateTime.Now - dateDebut).ToString(@"hh\:mm\:ss") + Environment.NewLine);
}

//répète la méthode de copie différentielle pour chaque répertoire
void FaireCopyDirectoryDiff()
{
    Console.WriteLine("Vous vous apprêter à faire un copie différentielle des répertoires susmentionnés dans le répertoire suivant : \n" + destinationValue + "\n");
    AppuyezPourContinuer();
    DateTime dateDebut = DateTime.Now;
    File.AppendAllText(log, "Début de la copie différentielle :" + dateDebut + Environment.NewLine);
    
    //pour chaque répertoire
    foreach (string folder in folders)
    {
        try
        {
            //afficher le répertoire à copier
            Console.WriteLine(folder);
            //copie le répertoire et ses sous-répertoires
            CopyDirectoryDiff(sourceValue + folder, destinationValue + folder);
        }
        //erreur si l'accès au dossier n'est pas autorisé
        catch (Exception e)
        {
            Console.WriteLine("Erreur : " + e.Message);
            //ajout de cette erreur dans le fichier log et dans la liste d'erreur
            string time = DateTime.Now.ToString();
            nbErreur++;
            listeErreur.Add(e.Message + " " + time);
        }
    }

    AfficherErreur(nbErreur, log);
    Console.WriteLine("\nTache terminee \n\nInformations dans le fichier log : \n" + log);
    
    //ajout des infos au fichier log
    File.AppendAllText(log, "Fin: " + DateTime.Now + Environment.NewLine);
    File.AppendAllText(log, SEPARATION + Environment.NewLine);
    File.AppendAllText(log, "Liste des répertoires concernés : " + Environment.NewLine);
    foreach (string folder in folders) { File.AppendAllText(log, sourceValue + folder + Environment.NewLine); }
    File.AppendAllText(log, SEPARATION + Environment.NewLine);
    File.AppendAllText(log, "Copie différentielle terminée." + "\nTemps d'éxecution : " + (DateTime.Now - dateDebut).ToString(@"hh\:mm\:ss") + Environment.NewLine);
    File.AppendAllText(log, "Nombre de fichiers mis à jour : " + nbFichiersModifier + Environment.NewLine + "Nombre de fichiers ignorés : " + nbFichierIgnore + Environment.NewLine);
}

//copie différentielle d'un répertoire - copie uniquement les fichiers modifiés
void CopyDirectoryDiff(string cheminSource, string cheminDestination)
{
    //si le répertoire de destination n'existe pas, le créer
    if (!Directory.Exists(cheminDestination))
    {
        Directory.CreateDirectory(cheminDestination);
    }

    //pour chaque fichier dans le répertoire source
    foreach (string fichier in Directory.GetFiles(cheminSource))
    {
        //si le fichier n'existe pas dans le répertoire de destination ou si la date de modification est différente du fichier existant dans le répertoire de destination
        if (!File.Exists(Path.Combine(cheminDestination, Path.GetFileName(fichier))) || File.GetLastWriteTime(fichier) != File.GetLastWriteTime(Path.Combine(cheminDestination, Path.GetFileName(fichier))))
        {
            //copie le fichier dans le répertoire de destination
            File.Copy(fichier, Path.Combine(cheminDestination, Path.GetFileName(fichier)), true);
            nbFichiersModifier += 1;
            Console.WriteLine("Copie de : " + fichier);
        }
        else
        {
            nbFichierIgnore += 1;
            Console.WriteLine(nbFichierIgnore + " fichiers ignorés : " + fichier);
        }
    }

    //pour chaque répertoire dans le répertoire source
    foreach (string repertoire in Directory.GetDirectories(cheminSource))
    {
        //si repertoire existe et n'a pas changer 
        if (Directory.Exists(Path.Combine(cheminDestination, Path.GetFileName(repertoire))) && Directory.GetLastWriteTime(repertoire) == Directory.GetLastWriteTime(Path.Combine(cheminDestination, Path.GetFileName(repertoire))))
        {
            //nbfichierignore = nbfichierignore + nombre de fichier dans le repertoire
            nbFichierIgnore += Directory.GetFiles(repertoire).Length;
            Console.WriteLine(nbFichierIgnore + " fichiers ignorés : " + repertoire);
        }
        else
        {
            //copie le répertoire et ses sous-répertoires
            CopyDirectoryDiff(repertoire, Path.Combine(cheminDestination, Path.GetFileName(repertoire)));
        }
    }
}

void CopyDirectory(string cheminSource, string cheminDestination)
{
    //si le répertoire de destination n'existe pas, le créer
    if (!Directory.Exists(cheminDestination))
    {
        Directory.CreateDirectory(cheminDestination);
    }
    //pour chaque fichier dans le répertoire source
    foreach (string fichier in Directory.GetFiles(cheminSource))
    {
        //copie le fichier dans le répertoire de destination
        File.Copy(fichier, Path.Combine(cheminDestination, Path.GetFileName(fichier)), true);
        Console.WriteLine("Copie de : " + fichier);
        nbFichiersModifier += 1;
    }
    //pour chaque répertoire dans le répertoire source
    foreach (string repertoire in Directory.GetDirectories(cheminSource))
    {
        //copie le répertoire dans le répertoire de destination
        CopyDirectory(repertoire, Path.Combine(cheminDestination, Path.GetFileName(repertoire)));
    }
}

//verification des fichiers complete ou rapide
void VerifDesFichiers()
{

    Console.WriteLine("\nProceder a la verification des fichiers :\n" 
        + "La vérification complète va compter le nombre de fichier dans chaque dossier. Cela permet de tester si l'accès est bien accordé.\n" 
        + "La vérification rapide ne parcours que les dossiers principaux en entrant pas dans les sous-dossier et fichiers.");
    Console.WriteLine("1 - Complete\n" + "2 - Rapide (uniquement les dossiers principaux)");
    int choixVerif = Convert.ToInt32(Console.ReadLine());

    switch (choixVerif)
    {
        case 1:
            Console.WriteLine("Vérification des fichiers en cours...");
            int NbFichiersVerifier = 0;

            //pour chaque dossier de folder.ini 
            foreach (string folder in folders)
            {
                try
                {
                    //compte le nombre de fichier contenus et dans ses sous-dossiers (AllDirectories)
                    int NbFichiers = Directory.GetFiles(sourceValue + folder, "*.*", SearchOption.AllDirectories).Length;
                    Console.WriteLine(sourceValue + folder + " : " + NbFichiers + " fichiers");
                    NbFichiersVerifier += NbFichiers;
                }
                //erreur si l'accès au dossier n'est pas autorisé
                catch (Exception e)
                {
                    Console.WriteLine("Erreur : " + e.Message);
                }
            }
            Console.WriteLine("Total de fichiers parcourus : " + NbFichiersVerifier);
            break;

        case 2:

            //pour chaque dossier de folder.ini 
            foreach (string folder in folders)
            {
                try
                {
                    //compte le nombre de fichiers contenus dans les dossiers principaux (TopDirectoryOnly)
                    int NbFichiers = Directory.GetFiles(sourceValue + folder, "*.*", SearchOption.TopDirectoryOnly).Length;
                    Console.WriteLine(sourceValue + folder + " : " + NbFichiers + " fichiers");
                }
                //erreur si l'accès au dossier n'est pas autorisé
                catch (Exception e)
                {
                    Console.WriteLine("Erreur : " + e.Message);
                }
            }
            
            break;

        default:
            Console.WriteLine("Choix incorrect");
            VerifDesFichiers();
            break;
    }
}

void AppuyezPourContinuer()
{
    Console.WriteLine("\nAppuyez sur C pour continuer, Q pour quitter");
    string choix = Console.ReadLine();
    switch (choix)
    {
        case "C":
        case "c":
            break;
        case "Q":
        case "q":
            Environment.Exit(0);
            break;
        default:
            Console.WriteLine("Choix incorrect");
            AppuyezPourContinuer();
            break;
    }
}