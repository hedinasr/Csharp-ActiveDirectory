using CommandLine;
using CommandLine.Text;

namespace Csharp_ActiveDirectory
{
    internal abstract class CommonSubOptions
    {
        [Option('d', "domain", Required = true, HelpText = "Active Directory domain")]
        public string Domain { get; set; }

        [Option('u', "username", Required = true, HelpText = "Active Directory username")]
        public string Username { get; set; }

        [Option('p', "password", Required = true, HelpText = "Active Directory password")]
        public string Password { get; set; }

        [Option('f', "file", Required = true, HelpText = "CSV file (example: -f profs.csv)")]
        public string File { get; set; }

        [Option('c', "create", HelpText = "Create users")]
        public bool Create { get; set; }

        [Option('r', "remove", HelpText = "Remove users")]
        public bool Delete { get; set; }

        [Option("create-directories", HelpText = "Create students home directories")]
        public bool CreateDirectories { get; set; }
    }

    internal class StudentsSubOptions : CommonSubOptions
    {
        [Option('y', "year", HelpText = "School year (example: -y 2016-2017)")]
        public string Year { get; set; }

        [Option('a', "archived", HelpText = "Archive the given user (example: -a <username>)")]
        public string Archived { get; set; }
    }

    internal class ProfessorsSubOptions : CommonSubOptions
    {
    }

    class Options
    {
        [HelpVerbOption]
        public string GetUsage(string verb)
        {
            return HelpText.AutoBuild(this, verb);
        }

        [VerbOption("students",
            HelpText = "Create students in Active Directory")]
        public StudentsSubOptions CreateStudents { get; set; }

        [VerbOption("professors",
            HelpText = "Create professors in Active Directory")]
        public ProfessorsSubOptions CreateProfessors { get; set; }
    }
}
