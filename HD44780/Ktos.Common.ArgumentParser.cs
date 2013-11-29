using System;
using System.Collections.Generic;
using System.Text;

namespace Ktos.Common
{
    class Argument
    {
        public string Name { get; set; }
        public List<string> Switches { get; set; }
    }

    public class ArgumentParser
    {
        private string[] args;        
        private Dictionary<string, string> expand;

        private List<Argument> parsedArguments;

        public static string DefaultArg = "--default";
        public List<string> Prefixes { get; set; }
        public List<char> ParamPrefixes { get; set; }
        public List<char> ParamSeparators { get; set; }

        public ArgumentParser(string[] args)
        {
            this.args = args;

            this.Prefixes = new List<string>();
            this.Prefixes.Add("--");
            this.Prefixes.Add("-");
            this.Prefixes.Add("/");

            this.ParamPrefixes = new List<char>();
            this.ParamPrefixes.Add('=');
            this.ParamPrefixes.Add(':');

            this.ParamSeparators = new List<char>();
            this.ParamSeparators.Add(',');
            
            expand = new Dictionary<string, string>();
        }

        public void AddExpanded(string shortSwitch, string longSwitch)
        {
            expand.Add(shortSwitch, longSwitch);
        }

        public void Parse()
        {
            parsedArguments = new List<Argument>();

            Argument actual = new Argument();
            actual.Name = ArgumentParser.DefaultArg;
            actual.Switches = new List<string>();            

            for (int i = 0; i < args.Length; i++)
            {
                // jeśli zaczyna się od / albo -- albo - to uznajemy, że jest dany argument jest przełącznikiem
                if (args[i].StartsWith("/") || args[i].StartsWith("--") || args[i].StartsWith("-"))
                {
                    // dodawanie poprzedniego przełącznika do listy już sparsowanych
                    parsedArguments.Add(actual);
                    
                    // wydobycie "czystej" nazwy przełącznika, bez ewentualnych prefiksów parametrów
                    string newKey;

                    if (args[i].IndexOf(':') > -1)
                        newKey = args[i].Substring(0, args[i].IndexOf(':'));
                    else if (args[i].IndexOf('=') > -1)
                        newKey = args[i].Substring(0, args[i].IndexOf('='));
                    else
                        newKey = args[i];
                    
                    // nowy przełącznik
                    actual = new Argument();
                    actual.Name = ExpandSwitch(newKey);
                    actual.Switches = new List<string>();

                    foreach (var p in this.ParamPrefixes)
                    {
                        if (args[i].IndexOf(p) > -1)
                        {
                            var switches = args[i].Substring(args[i].IndexOf(p) + 1).Split(this.ParamSeparators.ToArray(), StringSplitOptions.RemoveEmptyEntries);
                            actual.Switches.AddRange(switches);
                        }
                    }
                }
                else
                // w przeciwnym wypadku dany argument linii polecenia jest parametrem przełącznika
                {
                    // rozszerzanie tablicy i dodawanie
                    actual.Switches.Add(args[i]);
                }                
            }

            // adding final argument
            parsedArguments.Add(actual);
        }

        public string ExpandSwitch(string s)
        {
            string result = s;
            
            try
            {
                if (!expand.TryGetValue(s, out result))
                    return s;
                else
                    return result;
            }
            catch (ArgumentNullException)
            {
                return s;
            }            
        }

        public IEnumerable<string> GetSwitchArguments(string sw)
        {
            sw = ExpandSwitch(sw);            
            var r = parsedArguments.Find(x => x.Name == sw);

            if (r != null)
                return r.Switches;
            else
                throw new KeyNotFoundException("Specified switch not found");
        }

        public bool MatchRequiredCount(int requiredCount)
        {
            // jeśli jest tylko jeden, przełącznik znaczy "default", to zwracamy czy liczba jego
            // parametrów jest zgodna
            if (parsedArguments.Count == 1)
                return parsedArguments.Find(x => x.Name == ArgumentParser.DefaultArg).Switches.Count >= requiredCount;
            else
                return parsedArguments.Count >= requiredCount;
        }

        public bool SwitchExists(string sw)
        {
            return (parsedArguments.Find(x => x.Name == sw) != null);
        }

        public bool HelpRequested()
        {
            return SwitchExists("--help");
        }

        public bool VersionRequested()
        {
            return SwitchExists("--version");
        }

        public bool DebugRequested()
        {
            return SwitchExists("--debug");
        }
    }
}
