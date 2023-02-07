

/*********************  SCRIPT DE COPIE DE FICHIERS AVEC CONFIGURATION  *********************\
 * 
 * Alexandre Robert
 * 01/12/2022
 * Version 2.1.2 Upgraded log, cleaned main
 *
 * Utilise un document App.config.xml pour le repertoire source unique (le plus haut) depuis lequel copier et sa destination
 * Utilise un document folder.ini qui référencie le détail des répertoires (plus bas) à copier
 * 
 * Le résultat est reporté dans un fichier log (fichier créer par jour) dans le dossier 'Logs'
 * 
 *
 * Amélioration notées pour la prochaine version :
 * - Suppression des dialogues (pour utilisation à l'aide de TaskManager)
 * - le mode sera à préciser dans le fichier App.config.xml ("différentielle", "simple")
 * - la vérification sera optionnelle et à préciser dans le fichier App.config.xml ("complète", "aucune")
 *
 */