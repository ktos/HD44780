#region License
/*
 * Hd44780
 *
 * Copyright (C) Marcin Badurowicz 2013
 *
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files
 * (the "Software"), to deal in the Software without restriction,
 * including without limitation the rights to use, copy, modify, merge,
 * publish, distribute, sublicense, and/or sell copies of the Software,
 * and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS
 * BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN
 * ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE. 
 */
#endregion
using System;
using System.Collections.Generic;
using System.Text;

namespace Ktos.Common
{
    /// <summary>
    /// A class representing single argument in command-line arguments:
    ///
    /// It may be something like: /s, -s, --silent
    /// Or even: -s:something,something2
    /// Or even: -s=something
    /// Or: -s something something2
    /// </summary>
    class Argument
    {
        /// <summary>
        /// Argument name (in the longer form, will be expanded if expanded list is populated)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// List of argument switches
        /// </summary>
        public List<string> Switches { get; set; }
    }

    /// <summary>
    /// A class parsing command-line argument
    /// </summary>
    public class ArgumentParser
    {
        /// <summary>
        /// An pure string array argument list from application
        /// </summary>
        private string[] args;

        /// <summary>
        /// A list of expands for arguments - short form, expanded form
        /// </summary>
        private Dictionary<string, string> expand;

        /// <summary>
        /// A list of parsed arguments
        /// </summary>
        private List<Argument> parsedArguments;

        /// <summary>
        /// A "default" argument - when no argument is detected, all entries are going here
        /// </summary>
        public static string DefaultArg = "--default";

        /// <summary>
        /// List of prefixes for recognizing an argument
        /// </summary>
        public List<string> Prefixes { get; set; }

        /// <summary>
        /// List of prefixes for recognizing an argument's switch
        /// </summary>
        public List<char> ParamPrefixes { get; set; }

        /// <summary>
        /// List of separators between argument's switches (except space)
        /// </summary>
        public List<char> ParamSeparators { get; set; }

        /// <summary>
        /// Creates a new instance of ArgumentParser
        /// </summary>
        /// <param name="args">An argument array from command line</param>
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

        /// <summary>
        /// Adds expanded form for an argument
        /// </summary>
        /// <param name="shortSwitch">Short version, e.g. -s</param>
        /// <param name="longSwitch">Long version, e.g. --silent</param>
        public void AddExpanded(string shortSwitch, string longSwitch)
        {
            expand.Add(shortSwitch, longSwitch);
        }

        /// <summary>
        /// Parses an argument list
        /// </summary>
        public void Parse()
        {
            parsedArguments = new List<Argument>();

            Argument actual = new Argument();
            actual.Name = ArgumentParser.DefaultArg;
            actual.Switches = new List<string>();            

            for (int i = 0; i < args.Length; i++)
            {
                // if starts with /, -- or - it's probably a new argument
                if (args[i].StartsWith("/") || args[i].StartsWith("--") || args[i].StartsWith("-"))
                {
                    // actually parsed argument is added to parsed list, and we're creating a new one
                    parsedArguments.Add(actual);
                    
                    // argument name
                    string newKey;
                    if (args[i].IndexOf(':') > -1)
                        newKey = args[i].Substring(0, args[i].IndexOf(':'));
                    else if (args[i].IndexOf('=') > -1)
                        newKey = args[i].Substring(0, args[i].IndexOf('='));
                    else
                        newKey = args[i];
                    
                    // creating a new one
                    actual = new Argument();
                    actual.Name = ExpandSwitch(newKey); // expanding name
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
                // so it's not an argument, but a switch for previous argument
                {                    
                    actual.Switches.Add(args[i]);
                }                
            }

            // adding final argument
            parsedArguments.Add(actual);
        }

        /// <summary>
        /// Expands an argument from short form to long form
        /// </summary>
        /// <param name="s">Short form of argument</param>
        /// <returns>Long form of argument</returns>
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

        /// <summary>
        /// Gets a list of all switches for selected argument
        /// </summary>
        /// <param name="sw">Argument name</param>
        /// <returns>Collection of all switches</returns>
        public IEnumerable<string> GetSwitchArguments(string sw)
        {
            sw = ExpandSwitch(sw);            
            var r = parsedArguments.Find(x => x.Name == sw);

            if (r != null)
                return r.Switches;
            else
                throw new KeyNotFoundException("Specified switch not found");
        }

        /// <summary>
        /// Tests if required number of arguments is done
        /// </summary>
        /// <param name="requiredCount">A required number of arguments</param>
        /// <returns>True if a number of arguments is greater than requiredCount</returns>
        public bool MatchRequiredCount(int requiredCount)
        {
            // jeśli jest tylko jeden, przełącznik znaczy "default", to zwracamy czy liczba jego
            // parametrów jest zgodna
            if (parsedArguments.Count == 1)
                return parsedArguments.Find(x => x.Name == ArgumentParser.DefaultArg).Switches.Count >= requiredCount;
            else
                return parsedArguments.Count >= requiredCount;
        }

        /// <summary>
        /// Checks if switch of a name exists
        /// </summary>
        /// <param name="sw">Switch name</param>
        /// <returns>Returns true or false</returns>
        public bool SwitchExists(string sw)
        {
            sw = this.ExpandSwitch(sw);
            return (parsedArguments.Find(x => x.Name == sw) != null);
        }

        /// <summary>
        /// Checks if help was requested
        /// </summary>
        /// <returns></returns>
        public bool HelpRequested()
        {
            return SwitchExists("--help");
        }

        /// <summary>
        /// Checks if version information was requested
        /// </summary>
        /// <returns></returns>
        public bool VersionRequested()
        {
            return SwitchExists("--version");
        }

        /// <summary>
        /// Checks if debug mode was requested
        /// </summary>
        /// <returns></returns>
        public bool DebugRequested()
        {
            return SwitchExists("--debug");
        }
    }
}
