using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.XmlDiffPatch;
using log4net;

namespace QA.Common.TCApi
{
    public interface IValidator
    {
        ValidationResult validateResource(IResource p_resource, Object p_against, Dictionary<String, String> p_options, bool save);
        String extension();
    }

    public class ValidationResult
    {
        public bool Valid;
        public IList<String> Messages;

        public ValidationResult()
        {
            Valid = false;
            Messages = new List<String>();
        }
    }

    public class XmlValidator : IValidator
    {
        #region IValidator Members

        public virtual ValidationResult validateResource(IResource p_resource, object p_against, Dictionary<string, string> p_options, bool save)
        {
            ValidationResult result = new ValidationResult();
            if (save)
            {
                XmlDocument against = p_against as XmlDocument;
                Stream output_stream = p_resource.getStream(FileAccess.Write);
                against.Save(new XmlTextWriter(output_stream, null));
                result.Valid = true;
                result.Messages.Add("Output saved to resource with location: " + p_resource.Location);
            }
            else
            {
                XmlDocument expected = new XmlDocument();
                expected.Load(p_resource.getStream(FileAccess.Read));
                XmlDocument actual = p_against as XmlDocument;
                MemoryStream diffstream = new MemoryStream();
                XmlDiff comparator = new XmlDiff(getOptions(p_options));
                result.Valid = comparator.Compare(expected, actual, new XmlTextWriter(diffstream, null));
                diffstream.Position = 0;
                using (StreamReader buffer = new StreamReader(diffstream))
                {
                    String line;
                    while ((line = buffer.ReadLine()) != null)
                    {
                        result.Messages.Add(line);
                    }
                }
            }
            return result;
        }

        public virtual string extension()
        {
            return ".xml";
        }

        #endregion

        public virtual XmlDiffOptions getOptions(Dictionary<String, String> options)
        {
            // I really dislike storing all options using a bitwise system, but since we're using someone else's library...

            XmlDiffOptions retval = getOptionDefaults();
            Dictionary<String, XmlDiffOptions> all_options = new Dictionary<string, XmlDiffOptions>();
            all_options["IgnoreChildOrder"] = XmlDiffOptions.IgnoreChildOrder;
            all_options["IgnoreComments"] = XmlDiffOptions.IgnoreComments;
            all_options["IgnoreDtd"] = XmlDiffOptions.IgnoreDtd;
            all_options["IgnoreNamespaces"] = XmlDiffOptions.IgnoreNamespaces;
            all_options["IgnorePI"] = XmlDiffOptions.IgnorePI;
            all_options["IgnorePrefixes"] = XmlDiffOptions.IgnorePrefixes;
            all_options["IgnoreWhitespace"] = XmlDiffOptions.IgnoreWhitespace;
            all_options["IgnoreXmlDecl"] = XmlDiffOptions.IgnoreXmlDecl;

            foreach (String option_name in all_options.Keys)
            {
                if (options.ContainsKey(option_name))
                {
                    // Yes you could consolidate these nested if statements, but they make my brain hurt as they are!
                    if (options[option_name].ToLower() == "true")
                    {
                        if (((int)retval & (int)all_options[option_name]) == 0)
                        {
                            // set the option to true
                            retval = retval | all_options[option_name];
                        }
                    }
                    else
                    {
                        if (((int)retval & (int)all_options[option_name]) > 0)
                        {
                            // set the option to false
                            retval = retval ^ all_options[option_name];
                        }
                    }
                }
            }
            
            return retval;
        }

        public virtual XmlDiffOptions getOptionDefaults()
        {
            XmlDiffOptions retval = XmlDiffOptions.None |
                                    XmlDiffOptions.IgnoreComments |
                                    XmlDiffOptions.IgnorePI |
                                    XmlDiffOptions.IgnoreXmlDecl |
                                    XmlDiffOptions.IgnorePrefixes;

            return retval;
        }

    }

    public class SerializableValidator : XmlValidator
    {
        private static ILog logger = LogManager.GetLogger(typeof(SerializableValidator));
        public override string extension()
        {
            return ".serialized" + base.extension();
        }

        public override ValidationResult validateResource(IResource p_resource, object p_against, Dictionary<string, string> p_options, bool save)
        {
            logger.DebugFormat("Initializing XML Serializer for type {0}.", p_against.GetType().ToString());
            XmlSerializer worker = new XmlSerializer(p_against.GetType());
            logger.Debug("Creating memory buffer.");
            MemoryStream buffer = new MemoryStream();
            logger.DebugFormat("Serializing object of type {0} to XML.", p_against.GetType().ToString());
            worker.Serialize(buffer, p_against);
            logger.Debug("Seeking to beggining of buffer.");
            buffer.Seek(0, SeekOrigin.Begin);
            logger.Debug("Loading XMLDocument from memory buffer contents.");
            XmlDocument against = new XmlDocument();
            against.Load(buffer);
            logger.DebugFormat("Running XMLValidator on resource: {0}", p_resource.Location);
            return base.validateResource(p_resource, against, p_options, save);
        }

        public override XmlDiffOptions getOptionDefaults()
        {
            return base.getOptionDefaults() | XmlDiffOptions.IgnoreChildOrder;
        }
    }

    public class GuidListValidator : IValidator
    {

        #region IValidator Members

        public ValidationResult validateResource(IResource p_resource, object p_against, Dictionary<string, string> p_options, bool save)
        {
            ValidationResult retval = new ValidationResult();
            List<Guid> against_list = getGuidList(p_against);
            if (save)
            {
                // if we're saving the list
                retval.Valid = true;
                StreamWriter output = new StreamWriter(p_resource.getStream(FileAccess.Write));
                foreach (Guid guid in against_list)
                {
                    output.WriteLine(guid.ToString());
                }
                output.Close();
            }
            else
            {
                // Determine if order matters
                bool OrderMatters = false;
                if (p_options.ContainsKey("OrderMatters") && p_options["OrderMatters"].ToLower().Equals("true"))
                {
                    OrderMatters = true;
                }

                List<Guid> saved_list = new List<Guid>();
                StreamReader resource_reader = new StreamReader(p_resource.getStream(FileAccess.Read));
                String line = null;
                while((line = resource_reader.ReadLine()) != null)
                {
                    saved_list.Add(new Guid(line));
                }

                if (saved_list.Count != against_list.Count)
                {
                    retval.Valid = false;
                    retval.Messages.Add("Saved guid list has " + saved_list.Count + " elements and list provided has " + against_list.Count + " elements, they cannot be equal.");
                    dumpListsToMessages(retval, against_list, saved_list);
                    return retval;
                }

                if (OrderMatters)
                {
                    retval.Valid = true;
                    for (int i = 0; i < against_list.Count; i++)
                    {
                        if (against_list[i] != saved_list[i])
                        {
                            retval.Messages.Add("Guid from provided list " + against_list[i] + " does not match item from saved list " + saved_list[i]);
                            retval.Valid = false;
                        }
                    }
                }
                else
                {
                    retval.Valid = true;
                    foreach (Guid guid in against_list)
                    {
                        if (!saved_list.Contains(guid))
                        {
                            retval.Messages.Add("Guid " + guid + " from provided list is not in saved list.");
                            retval.Valid = false;
                        }
                    }
                    if (!retval.Valid)
                    {
                        dumpListsToMessages(retval, against_list, saved_list);
                    }
                }
            }

            return retval;
        }

        public string extension()
        {
            return ".guidlist.txt";
        }

        #endregion

        protected void dumpListsToMessages(ValidationResult result, List<Guid> p_against, List<Guid> p_saved)
        {
            result.Messages.Add("Provided list of guids:");
            foreach (Guid guid in p_against)
            {
                result.Messages.Add(" " + guid);
            }
            result.Messages.Add("Saved list of guids:");
            foreach (Guid guid in p_saved)
            {
                result.Messages.Add(" " + guid);
            }
        }

        protected List<Guid> getGuidList(Object p_against)
        {
            if (p_against.GetType() == typeof(List<Guid>))
            {
                return p_against as List<Guid>;
            }
            else if (p_against.GetType() == typeof(Guid[]))
            {
                List<Guid> retval = new List<Guid>((Guid[])p_against);
                return retval;
            }
            else
            {
                return null;
            }
        }
    }

}
