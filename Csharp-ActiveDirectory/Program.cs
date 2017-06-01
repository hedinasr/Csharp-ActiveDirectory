using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Security.AccessControl;

namespace Csharp_ActiveDirectory
{
    class Program
    {
        static void Main(string[] args)
        {
            string invokeVerb = string.Empty;
            object invokeVerbInstance = null;

            // Contient les numéros des classes
            HashSet<string> classes = new HashSet<string>();
            List<string[]> students = null;
            List<string[]> professors = null;

            var options = new Options();
            if (!CommandLine.Parser.Default.ParseArgumentsStrict(args, options,
                (verb, subOptions) =>
                {
                    invokeVerb = verb;
                    invokeVerbInstance = subOptions;
                },
                () => { Console.WriteLine("Example: ADGestion.exe students -d grp05.local -u admin -p admin -f students.csv -y 2016-2017 -c"); }))
            {
                Environment.Exit(CommandLine.Parser.DefaultExitCodeFail);
            }

            if (invokeVerb == "students")
            {
                var subOptions = (StudentsSubOptions)invokeVerbInstance;
                ActiveDirectory activeDirectory = new ActiveDirectory(subOptions.Username, subOptions.Password);

                try
                {
                    students = ReadCSV(subOptions.File);
                }
                catch (Exception Ex)
                {
                    Console.WriteLine(Ex.Message);
                }

                if (subOptions.Create)
                {
                    Console.WriteLine("Création des étudiants");

                    if (!activeDirectory.IsGroupExisting("Élèves"))
                    {
                        // Création du groupe Élèves
                        activeDirectory.CreateNewGroup("Élèves", "Groupe élèves", true);
                    }

                    foreach (var student in students)
                    {
                        // On construit l'utilisateur à partir du CSV
                        var id = student[0];
                        var surname = student[1];
                        var givenName = student[2];
                        var birthday = student[3];
                        var classe = student[4];
                        var username = ConstructUsername(givenName, surname);

                        classes.Add(classe);

                        // On créer l'utilisateur s'il n'existe pas déjà
                        activeDirectory.CreateUser(username, "SRIVéà&è", givenName, surname, id);

                        // On le rajoute au groupe
                        try
                        {
                            activeDirectory.AddUserToGroup(username, "Élèves");
                        }
                        catch (Exception Ex)
                        {
                            Console.WriteLine(Ex.Message);
                        }
                    }
                }

                if (subOptions.Delete)
                {
                    Console.WriteLine("Suppression des étudiants");

                    students.ForEach(x => activeDirectory.DeleteUser(ConstructUsername(x[2], x[1])));
                }
            }

            if (invokeVerb == "professors")
            {
                var subOptions = (ProfessorsSubOptions)invokeVerbInstance;
                ActiveDirectory activeDirectory = new ActiveDirectory(subOptions.Username, subOptions.Password);

                try
                {
                    professors = ReadCSV(subOptions.File);
                }
                catch (Exception Ex)
                {
                    Console.WriteLine(Ex.Message);
                }

                if (subOptions.Create)
                {
                    Console.WriteLine("Création des professeurs");

                    if (!activeDirectory.IsGroupExisting("Professeurs"))
                    {
                        // Création du groupe Professeurs
                        activeDirectory.CreateNewGroup("Professeurs", "Groupe professeurs", true);
                    }

                    foreach (var prof in professors)
                    {
                        var surname = prof[0];
                        var givenName = prof[1];
                        var username = ConstructUsername(givenName, surname);

                        activeDirectory.CreateUser(username, "SRIVéà&è", givenName, surname, null);

                        try
                        {
                            activeDirectory.AddUserToGroup(username, "Professeurs");
                        }
                        catch (Exception Ex)
                        {
                            Console.WriteLine(Ex.Message);
                        }
                    }
                }

                if (subOptions.Delete)
                {
                    Console.WriteLine("Suppression des professeurs");

                    professors.ForEach(x => activeDirectory.DeleteUser(ConstructUsername(x[1], x[0])));
                }
            }

            if (((CommonSubOptions)invokeVerbInstance).CreateDirectories)
            {
                Console.WriteLine("Création des dossiers");

                var year = "2016-2017";
                var subOptions = (CommonSubOptions)invokeVerbInstance;

                Directory.CreateDirectory("c:\\" + year); // Création du dossier principal
                // TODO mettre ce dossier dans un dossier partagé (pas de c:)

                // Droits d'accès pour le dossier <année>/<classe>/public
                DirectorySecurity secRules = new DirectorySecurity();

                secRules.AddAccessRule(new FileSystemAccessRule(@subOptions.Domain + "\\Professeurs", FileSystemRights.WriteData, AccessControlType.Allow));
                secRules.AddAccessRule(new FileSystemAccessRule(@subOptions.Domain + "\\Élèves", FileSystemRights.WriteData, AccessControlType.Allow));
                secRules.AddAccessRule(new FileSystemAccessRule(@subOptions.Domain + "\\Administrateur", FileSystemRights.FullControl, AccessControlType.Allow));
                
                foreach (var classe in classes)
                {
                    Directory.CreateDirectory("c:\\" + year + "\\" + classe);
                    Directory.CreateDirectory("c:\\" + year + "\\" + classe + "\\public", secRules);
                }

                foreach (var student in students)
                {
                    // Droit d'accès pour les dossiers des élèves
                    DirectorySecurity secRules2 = new DirectorySecurity();

                    secRules2.AddAccessRule(new FileSystemAccessRule(@subOptions.Domain + "\\Administrateur", FileSystemRights.FullControl, AccessControlType.Allow));
                    secRules2.AddAccessRule(new FileSystemAccessRule(@subOptions.Domain + "\\Professeurs", FileSystemRights.Read, AccessControlType.Allow));
                    secRules2.AddAccessRule(new FileSystemAccessRule(@subOptions.Domain + "\\" + ConstructUsername(student[2], student[1]), FileSystemRights.FullControl, AccessControlType.Allow));

                    // Dossier par élève par classe
                    Directory.CreateDirectory("c:\\" + year + "\\" + student[4] + "\\" + ConstructUsername(student[2], student[1]), secRules2);

                    // Dossier par élève
                    Directory.CreateDirectory("c:\\élève\\" + ConstructUsername(student[2], student[1]), secRules);
                }

                foreach (var prof in professors)
                {
                    var username = ConstructUsername(prof[1], prof[0]);

                    // dossier public
                    DirectorySecurity secRules2 = new DirectorySecurity();
                    secRules2.AddAccessRule(new FileSystemAccessRule(@subOptions.Domain + "\\" + username, FileSystemRights.FullControl, AccessControlType.Allow));
                    secRules2.AddAccessRule(new FileSystemAccessRule(@subOptions.Domain + "\\Administrateur", FileSystemRights.FullControl, AccessControlType.Allow));
                    secRules2.AddAccessRule(new FileSystemAccessRule(@subOptions.Domain + "\\Professeurs", FileSystemRights.Read, AccessControlType.Allow));
                    secRules2.AddAccessRule(new FileSystemAccessRule(@subOptions.Domain + "\\Élèves", FileSystemRights.Read, AccessControlType.Allow));
                    Directory.CreateDirectory(@"c:\\professeurs\\" + username + "\\public", secRules2);

                    // dossier dépôt
                    DirectorySecurity secRules3 = new DirectorySecurity();
                    secRules3.AddAccessRule(new FileSystemAccessRule(@subOptions.Domain + "\\" + username, FileSystemRights.FullControl, AccessControlType.Allow));
                    secRules3.AddAccessRule(new FileSystemAccessRule(@"ananasr.local\Administrateur", FileSystemRights.FullControl, AccessControlType.Allow));
                    secRules3.AddAccessRule(new FileSystemAccessRule(@"ananasr.local\Professeurs", FileSystemRights.WriteData, AccessControlType.Allow));
                    secRules3.AddAccessRule(new FileSystemAccessRule(@"ananasr.local\Élèves", FileSystemRights.WriteData, AccessControlType.Allow));
                    Directory.CreateDirectory(@"c:\\professeurs\\" + username + "\\dépôt", secRules3);

                    // dossier perso
                    DirectorySecurity secRules4 = new DirectorySecurity();
                    secRules4.AddAccessRule(new FileSystemAccessRule(@subOptions.Domain + "\\" + username, FileSystemRights.FullControl, AccessControlType.Allow));
                    secRules4.AddAccessRule(new FileSystemAccessRule(@subOptions.Domain + "\\Administrateur", FileSystemRights.FullControl, AccessControlType.Allow));
                    Directory.CreateDirectory(@"c:\\professeurs\" + username, secRules4);
                }
            }
        }

        static List<string[]> ReadCSV(string filename)
        {
            using (TextFieldParser parser = new TextFieldParser(filename))
            {
                List<string[]> content = new List<string[]>();
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(";");
                parser.SetFieldWidths(6);
                parser.ReadLine(); // on saute la première ligne

                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();
                    // add fields to the list with blanks removed
                    content.Add(fields.Where(x => !string.IsNullOrEmpty(x)).ToArray());
                }

                return content;
            }
        }

        static string ConstructUsername(string givenName, string surname)
        {
            var username = givenName[0] + surname;
            return username.ToLower();
        }
    }
}
