

/********************* FILE COPY SCRIPT WITH CONFIGURATION *********************\
 * 
 * Alexandre Robert
 * 21/02/2023
 * V2.2.1: Paths in code are automated and no longer hard-coded
 * V2.2 : Added suppression of files in destination folder if they don't exist in source folder
 *
 * Uses an App.config.xml document for the single (top) source folder to copy from and its destination folder
 * Uses a folder.ini document referencing the details of the (lower) directories to be copied
 * 
 * The result is reported in a log file (file created per day) in the 'Logs' folder
 * 
 *
 * Improvements noted for next version :
 * - Removal of dialogs (for use with TaskManager)
 * mode to be specified in App.config.xml file ("différentielle", "simple")
 * verification will be optional and to be specified in App.config.xml ("complète", "aucune")
 *
 */
