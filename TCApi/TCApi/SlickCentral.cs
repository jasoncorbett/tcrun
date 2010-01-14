using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CookComputing.XmlRpc;

namespace QA.Common.TCApi
{
    public class AbstractResult
    {
        [XmlRpcMember("successful")]
        public bool SuccessfulResult;

        [XmlRpcMember("errors")]
        public String[] Errors;
    }

    public class ListResult<T> : AbstractResult
    {
        [XmlRpcMember("data")]
        public T[] ListData;
    }

    public class SingleResult<T> : AbstractResult
    {
        [XmlRpcMember("data")]
        public T Data;
    }

    public class SlickProjectInfo
    {
        [XmlRpcMember("id")]
        public int ProjectId;

        [XmlRpcMember("name")]
        public String Name;

        [XmlRpcMember("default_feature")]
        public int DefaultFeatureGroupId;

        [XmlRpcMember("default_build")]
        public int DefaultBuildId;

        [XmlRpcMember("default_testplan")]
        public int DefaultTestPlanId;

        [XmlRpcMember("motd")]
        public String Motd;
    }

    public class SlickBuildInfo
    {
        [XmlRpcMember("builds.id")]
        public int BuildId;

        [XmlRpcMember("builds.built")]
        public DateTime BuildTimestamp;

        [XmlRpcMember("builds.name")]
        public String BuildName;

        [XmlRpcMember("builds.release_")]
        public int ReleaseId;

        [XmlRpcMember("releases.name")]
        public String ReleaseName;
    }

    public class SlickResultCreationInfo
    {
        [XmlRpcMember("result.id"),
         XmlRpcMissingMapping(MappingAction.Ignore)]
        public int? ResultId;
    }

    public class SlickResultInfo
    {
        [XmlRpcMember("build")]
        public int BuildId;

        [XmlRpcMember("test_name")]
        public String TestName;

        [XmlRpcMember("result_type")]
        public String Result;

        [XmlRpcMember("test_plan")]
        public int TestPlanId;
    }

    public class SlickTestPlanInfo
    {
        [XmlRpcMember("id")]
        public int TestPlanId;

        [XmlRpcMember("name")]
        public String Name;

        [XmlRpcMember("file_name")]
        public String FileName;
    }

    [XmlRpcUrl("http://slick.homestead-corp.com/slick/services/call/xmlrpc")]
    public interface ISlickCentral
    {
        [XmlRpcMethod]
        ListResult<SlickProjectInfo> read_projects_by_name(String name);

        [XmlRpcMethod]
        ListResult<SlickBuildInfo> read_builds(int project_id);

        [XmlRpcMethod]
        ListResult<SlickBuildInfo> read_builds(int project_id, int release_id);

        [XmlRpcMethod]
        ListResult<SlickTestPlanInfo> read_all_test_plans();

        [XmlRpcMethod]
        ListResult<SlickTestPlanInfo> read_all_test_plans(int project_id);

        [XmlRpcMethod]
        SingleResult<SlickResultCreationInfo> create_result(SlickResultInfo result_info);

        [XmlRpcMethod]
        SingleResult<SlickBuildInfo> read_build_by_id(int build_id);
    }
}
