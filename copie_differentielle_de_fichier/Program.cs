/*
 * Alexandre Robert
 * 01/12/2022
 * 
 * Version 2.2 Added suppression of files in destination folder if they don't exist in source folder
 * 
 * v2.2.1 : AppConfig, folder.ini et log : chemin automatique
 * 
 * COPIE DE DOSSIERS ET FICHIERS AVEC CONFIGURATION
 * 
 * Utilise un document App.config.xml pour le repertoire source unique (le plus haut) depuis lequel copier et sa destination
 * Utilise un document folder.ini qui référencie le détail des répertoires (plus bas) à copier
 * 
 */

using System.Xml;

// Import de la configuration
XmlDocument AppConfig = new XmlDocument();
AppConfig.Load("K:\\08_Informatique\\15_Scripts\\Sauvegarde\\copie_differentielle_de_fichier\\App.Config.xml");
XmlNodeList source = AppConfig.GetElementsByTagName("source");
XmlNodeList destination = AppConfig.GetElementsByTagName("destination");

//declaration des variables globales
string repertoireSource = source[0].InnerText;
string repertoireDestination = destination[0].InnerText;
int nbFichiersModifier = 0;
int nbFichierIgnore = 0;
int nbFichierSupprime = 0;
int nbErreur = 0;
string[] liste_dossier_a_copier = File.ReadAllLines("K:\\08_Informatique\\15_Scripts\\Sauvegarde\\copie_differentielle_de_fichier\\folder.ini");
List<string> listeErreur = new List<string>();
string log = "K:\\08_Informatique\\15_Scripts\\Sauvegarde\\copie_differentielle_de_fichier\\logs\\" + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
const string SEPARATION = "--------------------------------------------------";


//************* PARAMETRAGE UTILISATEUR *************//

//affichage infos
Console.WriteLine("Informations définies dans le fichier App.Config.xml : \n");
Console.WriteLine("SOURCE : " + repertoireSource);
Console.WriteLine("DESTINATION : " + repertoireDestination + "\n" + SEPARATION + "\n");
Console.WriteLine("La copie concerne l'entièreté des fichiers contenus dans les répertoires suivants :\n");
foreach (string dossier_principal in liste_dossier_a_copier)
{
    Console.WriteLine(dossier_principal);
}

// vérification des fichier avant copie
VerifDesFichiers();

// Choix de la méthode de copie
Console.WriteLine("\nChoix de la méthode de copie :");
Console.WriteLine("1 - Copie simple");
Console.WriteLine("2 - Copie différentielle");
int choixMethode = Convert.ToInt32(Console.ReadLine());

// Choix de la suppression
Console.WriteLine("\nVoulez-vous supprimer les fichiers dans le répertoire de destination qui ne sont pas présents dans le répertoire source ? (O/N)");
string choixSuppression = Console.ReadLine();
bool suppression_est_demandee = false;
if (choixSuppression == "O" || choixSuppression == "o")
{
    suppression_est_demandee = true;
}

File.AppendAllText(log, "****************************************************************" + Environment.NewLine);

// Effectue la copie selon le choix
switch (choixMethode)
{
    case 1:
        FaireCopieSimple();
        break;
    case 2:
        FaireCopieDifferentielle();
        break;
    default:
        Console.WriteLine("Choix incorrect");
        break;
}

//****************** METHODES ******************//

// Répète la méthode de copie simple pour chaque répertoire
void FaireCopieSimple()
{
    Console.WriteLine("Vous vous apprêtez à faire un copie simple des répertoires susmentionnés dans le répertoire suivant : \n" + repertoireDestination + "\n");
    AppuyezPourContinuer();
    DateTime dateDebut = DateTime.Now;
    File.AppendAllText(log, "Début de la copie simple :" + DateTime.Now + Environment.NewLine);
    
    foreach (string dossier in liste_dossier_a_copier)
    {
        try
        {
            CopieSimple(repertoireSource + dossier, repertoireDestination + dossier);
            if (suppression_est_demandee) SupprimerFichiers(repertoireSource + dossier, repertoireDestination + dossier);
        }
        catch (Exception e)
        {
            string time = DateTime.Now.ToString();
            nbErreur++;
            listeErreur.Add(e.Message + " " + time);
        }
    }
    
    Afficher_erreur(nbErreur, log);
    RemplirLog(dateDebut, log);
}

//répète la méthode de copie différentielle pour chaque répertoire
void FaireCopieDifferentielle()
{
    Console.WriteLine("Vous vous apprêtez à faire un copie différentielle des répertoires susmentionnés dans le répertoire suivant : \n" + repertoireDestination + "\n");
    AppuyezPourContinuer();
    DateTime dateDebut = DateTime.Now;
    File.AppendAllText(log, "Début de la copie différentielle :" + dateDebut + Environment.NewLine);

    foreach (string dossier in liste_dossier_a_copier)
    {
        try
        {
            CopieDifferentielle(repertoireSource + dossier, repertoireDestination + dossier);
            if (suppression_est_demandee) SupprimerFichiers(repertoireSource + dossier, repertoireDestination + dossier); 
        }
        
        catch (Exception e)
        {
            string time = DateTime.Now.ToString();
            nbErreur++;
            listeErreur.Add(e.Message + " " + time);
        }
    }

    Afficher_erreur(nbErreur, log);
    RemplirLog(dateDebut, log);
}

//copie différentielle d'un répertoire - copie uniquement les fichiers modifiés
void CopieDifferentielle(string cheminSource, string cheminDestination)
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
            CopieDifferentielle(repertoire, Path.Combine(cheminDestination, Path.GetFileName(repertoire)));
        }
    }
}

void CopieSimple(string cheminSource, string cheminDestination)
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
        CopieSimple(repertoire, Path.Combine(cheminDestination, Path.GetFileName(repertoire)));
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

            foreach (string dossier in liste_dossier_a_copier)
            {
                try
                {
                    //compte le nombre de fichier contenus et dans ses sous-dossiers (AllDirectories)
                    int NbFichiers = Directory.GetFiles(repertoireSource + dossier, "*.*", SearchOption.AllDirectories).Length;
                    Console.WriteLine(repertoireSource + dossier + " : " + NbFichiers + " fichiers");
                    NbFichiersVerifier += NbFichiers;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Erreur : " + e.Message);
                }
            }
            Console.WriteLine("Total de fichiers parcourus : " + NbFichiersVerifier);
            break;

        case 2:

            foreach (string dossier in liste_dossier_a_copier)
            {
                try
                {
                    //compte le nombre de fichiers contenus dans les dossiers principaux (TopDirectoryOnly)
                    int NbFichiers = Directory.GetFiles(repertoireSource + dossier, "*.*", SearchOption.TopDirectoryOnly).Length;
                    Console.WriteLine(repertoireSource + dossier + " : " + NbFichiers + " fichiers");
                }
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

//supprime les fichiers dans le répertoire de destination qui ne sont pas présents dans le répertoire source
void SupprimerFichiers(string cheminSource, string cheminDestination)
{
    //pour chaque fichier dans le répertoire de destination
    foreach (string fichier in Directory.GetFiles(cheminDestination))
    {
        //si le fichier n'existe pas dans le répertoire source
        if (!File.Exists(Path.Combine(cheminSource, Path.GetFileName(fichier))))
        {
            //supprime le fichier
            File.Delete(fichier);
            Console.WriteLine("Suppression de : " + fichier);
            nbFichierSupprime += 1;
        }
    }
    //pour chaque répertoire dans le répertoire de destination
    foreach (string repertoire in Directory.GetDirectories(cheminDestination))
    {
        //si le répertoire n'existe pas dans le répertoire source
        if (!Directory.Exists(Path.Combine(cheminSource, Path.GetFileName(repertoire))))
        {
            //supprime le répertoire
            Directory.Delete(repertoire, true);
            Console.WriteLine("Suppression de : " + repertoire);
            nbFichierSupprime += 1;
        }
        else
        {
            //sinon, supprime les fichiers dans le répertoire de destination qui ne sont pas présents dans le répertoire source
            SupprimerFichiers(Path.Combine(cheminSource, Path.GetFileName(repertoire)), repertoire);
        }
    }
}

void Afficher_erreur(int nbErreur, string log)
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

void RemplirLog(DateTime dateDebut, string log)
{
    Console.WriteLine("\nTache terminee \n\nInformations dans le fichier log : \n" + log);

    //ajout des infos dans le fichier log
    File.AppendAllText(log, "Fin: " + DateTime.Now + Environment.NewLine);
    File.AppendAllText(log, SEPARATION + Environment.NewLine);
    File.AppendAllText(log, "Liste des répertoires concernés : " + Environment.NewLine);
    foreach (string dossier in liste_dossier_a_copier) { File.AppendAllText(log, repertoireSource + dossier + Environment.NewLine); }
    File.AppendAllText(log, SEPARATION + Environment.NewLine);

    if (choixMethode == 2)
    {
        File.AppendAllText(log, "Copie différentielle terminée : \nNombre de fichiers copiés : " + nbFichiersModifier + "\nNombre de fichiers ignorés : " + nbFichierIgnore);
    }
    else
    {
        File.AppendAllText(log, "Copie terminée : \nNombre de fichiers copiés : " + nbFichiersModifier);
    }

    if (suppression_est_demandee)
    {
        File.AppendAllText(log, "\nNombre de fichiers supprimés : " + nbFichierSupprime + Environment.NewLine);
    }
    
    File.AppendAllText(log, "\nTemps d'éxecution : " + (DateTime.Now - dateDebut).ToString(@"hh\:mm\:ss") + Environment.NewLine);
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