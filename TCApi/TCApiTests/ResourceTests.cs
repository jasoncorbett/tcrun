using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using QA.Common.TCApi;

namespace QA.Common.TCApi.Tests.Resource
{
    [TCName("Create a new resource"),
     TCGroup("TCApi/Resource"),
     TCGroup("TCApi"),
     TCGuid("1B71D086-F530-41fe-86DF-8907B77A8182")]
    public class FileResourceCreationTest : AbstractTestCase
    {
        public static String RESOURCE_STRING_TEST = "foobar";
        private String m_tmp_dir;

        public override void tcSetup(Dictionary<string, string> configuration)
        {
            base.tcSetup(configuration);
            m_tmp_dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            TCLog.AuditFormat("Creating temporary directory for Resource test: {0}", m_tmp_dir);
            Directory.CreateDirectory(m_tmp_dir);
            TCLog.AuditFormat("Directory {0} exists: {1}", m_tmp_dir, Directory.Exists(m_tmp_dir));
        }

        public override TEST_RESULTS tcDoTest()
        {
            String env_name = "Cheese";
            TCApiConfiguration config = new TCApiConfiguration(tc_info);
            config["Environment.Name"] = env_name;
            IResourceFactory resource_factory = new FileResourceFactory(m_tmp_dir, config);
            IResource resource = resource_factory.loadResource(this.GetType(), "example");
            if (resource == null)
            {
                TCLog.Audit("TEST FAILURE: resource retrieved from factory is null.");
                return TEST_RESULTS.Fail;
            }

            TCLog.AuditFormat("resource by name of \"example\" exists: {0}", resource.Exists);

            if (!resource.Location.Contains(env_name))
            {
                TCLog.AuditFormat("TEST FAILURE: Resource location should be dependent on Environment.Name '{0}', was: '{1}'", env_name, resource.Location);
                return TEST_RESULTS.Fail;
            }
            TCLog.AuditFormat("Location '{0}' contains environment name '{1}'.", resource.Location, env_name);
            TCLog.AuditFormat("Opening a write stream to the resource with location: {0}", resource.Location);

            try
            {
                StreamWriter resource_stream = new StreamWriter(resource.getStream(FileAccess.Write));
                resource_stream.WriteLine(RESOURCE_STRING_TEST);
                resource_stream.Close();
            }
            catch (Exception ex)
            {
                TCLog.Audit("TEST FAILURE: Failed to write to resource with location " + resource.Location + ": ", ex);
                return TEST_RESULTS.Fail;
            }

            if(!resource.Exists)
            {
                TCLog.AuditFormat("TEST FAILURE: resource by name of \"example\" exists: {0}", resource.Exists);
                return TEST_RESULTS.Fail;
            }
            TCLog.AuditFormat("resource by name of \"example\" exists: {0}", resource.Exists);

            TCLog.AuditFormat("Opening a read stream to the resource with location: {0}", resource.Location);
            try
            {
                StreamReader resource_stream = new StreamReader(resource.getStream(FileAccess.Read));
                String actual = resource_stream.ReadLine();
                resource_stream.Close();

                if (RESOURCE_STRING_TEST != actual)
                {
                    TCLog.AuditFormat("TEST FAILURE: resource should have had contents of '{0}', but had '{1}'.", RESOURCE_STRING_TEST, actual);
                    return TEST_RESULTS.Fail;
                }
                TCLog.AuditFormat("resource contents as expected: {0}", actual);
            }
            catch (Exception ex)
            {
                TCLog.Audit("TEST FAILURE: Unable to read from resource with location " + resource.Location + ": ", ex);
                return TEST_RESULTS.Fail;
            }
                    
            return TEST_RESULTS.Pass;
        }

        public override void tcCleanup()
        {
            base.tcCleanup();
            TCLog.AuditFormat("Removing temporary directory for Resource test: {0}", m_tmp_dir);
            Directory.Delete(m_tmp_dir, true);
            TCLog.AuditFormat("Directory {0} exists: {1}", m_tmp_dir, Directory.Exists(m_tmp_dir));
        }
    }

    [TCName("Test the XmlValidator"),
     TCGroup("TCApi"),
     TCGroup("TCApi/Validator"),
     TCGuid("389CB75D-EF04-4d47-AC8C-98466B63BAAF")]
    public class XmlValidatorTest : AbstractTestCase
    {
        private string same = @"
                                  <test>
                                   <this />
                                   <is />
                                   <a />
                                   <simpletest />
                                  </test>";
        private string whitespace = @"
                                  <test>

                                   <this />

                                   <is />

                                   <a />

                                   <simpletest />

                                  </test>";
        private string comment = @"
                                  <test>
                                   <this />
                                   <is />
                                   <a />
                                   <!-- this shouldn't break anything -->
                                   <simpletest />
                                  </test>";
        private string different = @"
                                  <test>
                                   <this />
                                   <is />
                                   <a />
                                   <complextest />
                                  </test>";

        public override TEST_RESULTS tcDoTest()
        {
            XmlDocument sameXml = new XmlDocument();
            XmlDocument whitespaceXml = new XmlDocument();
            XmlDocument commentXml = new XmlDocument();
            XmlDocument differentXml = new XmlDocument();
            sameXml.LoadXml(same);
            whitespaceXml.LoadXml(whitespace);
            commentXml.LoadXml(comment);
            differentXml.LoadXml(different);

            TCLog.Audit("Validating identical xml.");
            if (!validate(sameXml, typeof(XmlValidator)))
            {
                TCLog.Audit("TEST FAILURE: The same xml was not the same!");
                return TEST_RESULTS.Fail;
            }
            if (!SaveValidation)
            {
                // we don't want to save these documents as the result.xml
                TCLog.Audit("Validating xml with different whitespace");
                if (!validate(whitespaceXml, typeof(XmlValidator)))
                {
                    TCLog.Audit("TEST FAILURE: The whitespace xml did not validate!");
                    return TEST_RESULTS.Fail;
                }
                TCLog.Audit("Validating xml with a comment in it.");
                if (!validate(commentXml, typeof(XmlValidator)))
                {
                    TCLog.Audit("TEST FAILURE: The comment xml did not validate!");
                    return TEST_RESULTS.Fail;
                }
                TCLog.Audit("Validating differnt xml (should fail validation).");
                if (validate(differentXml, typeof(XmlValidator)))
                {
                    TCLog.Audit("TEST FAILURE: The different xml validated (very bad)!");
                    return TEST_RESULTS.Fail;
                }
            }
            return TEST_RESULTS.Pass;
        }
    }

    [TCName("Test the SerializableValidator"),
     TCGroup("TCApi"),
     TCGroup("TCApi/Validator"),
     TCGuid("58dd6519-e8d9-461d-91a6-3b5f0b480282")]
    public class SerializableValidatorTest : AbstractTestCase
    {
        [Serializable,
         DataContract(Name="SerializationTest")]
        public class SerializationTest
        {
            [DataMember]
            public String Name;
            [OptionalField]
            public String Description;
            [DataMember]
            public int Number;
            [DataMember]
            public Guid AGuid;
        }

        private SerializationTest original;
        private SerializationTest identical;
        private SerializationTest propertyMissing;
        private SerializationTest propertyChange;


        public override void tcSetup(Dictionary<string, string> configuration)
        {
            base.tcSetup(configuration);
            original = new SerializationTest();
            identical = new SerializationTest();
            propertyMissing = new SerializationTest();
            propertyChange = new SerializationTest();

            original.Name = "Foo";
            identical.Name = "Foo";
            propertyMissing.Name = "Foo";
            propertyChange.Name = "Bar";

            original.Description = "Description\r\nOf\r\nA\r\nSerialized\r\nTest\r\n";
            identical.Description = "Description\r\nOf\r\nA\r\nSerialized\r\nTest\r\n";
            propertyMissing.Description = null;
            propertyChange.Description = "Description\r\nOf\r\nA\r\nSerialized\r\nTest";

            original.Number = 247;
            identical.Number = 247;
            propertyMissing.Number = 247;
            propertyChange.Number = 248;

            original.AGuid = new Guid("77c4832b-0044-4326-a61a-1360a194fdd2");
            identical.AGuid = new Guid("77c4832b-0044-4326-a61a-1360a194fdd2");
            propertyChange.AGuid = Guid.NewGuid();
            propertyMissing.AGuid = new Guid("77c4832b-0044-4326-a61a-1360a194fdd2");
        }


        public override TEST_RESULTS tcDoTest()
        {
            TCLog.Audit("Validating original object");
            if (!validate(original, typeof(SerializableValidator)))
            {
                TCLog.Audit("TEST FAILURE: validation of original object failed.");
                return TEST_RESULTS.Fail;
            }

            if (!SaveValidation)
            {
                // we only want the original to be saved, not any of these
                TCLog.Audit("Validating identical object (identical properties)");
                if (!validate(identical, typeof(SerializableValidator)))
                {
                    TCLog.Audit("TEST FAILURE: validation of identical object failed!");
                    return TEST_RESULTS.Fail;
                }
                TCLog.Audit("Validating object with differences.");
                if (validate(propertyChange, typeof(SerializableValidator)))
                {
                    TCLog.Audit("TEST FAILURE: validation of changed object succeeded!");
                    return TEST_RESULTS.Fail;
                }
                TCLog.Audit("Validating object with empty GUID.");
                if (validate(propertyMissing, typeof(SerializableValidator)))
                {
                    TCLog.Audit("TEST FAILURE: validation of empty guid object succeeded!");
                    return TEST_RESULTS.Fail;
                }
            }
            return TEST_RESULTS.Pass;
        }
    }
}
