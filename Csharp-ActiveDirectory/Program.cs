using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csharp_ActiveDirectory
{
    class Program
    {
        static void Main(string[] args)
        {
            string invokeVerb = string.Empty;
            object invokeVerbInstance = null;

            // Contient toutes les numéros des classes
            HashSet<string> classes = new HashSet<string>();

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
                Console.WriteLine("Création des étudiants");
            }

            if (invokeVerb == "professors")
            {
                var subOptions = (ProfessorsSubOptions)invokeVerbInstance;
                Console.WriteLine("Création des professeurs");
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
