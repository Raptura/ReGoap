using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MH;

namespace ReGoap.Unity.FactoryExample.Planners
{
    public class FactoryPlannerMgr : ReGoapPlannerManager<string, object>
    {

    }

    public class GOAPStateName
    {
        public const string HasStock = nameof(HasStock);
        public const string HasStockSpace = nameof(HasStockSpace);
        public const string MakeStock = nameof(MakeStock);
        public const string CanLoan = nameof(CanLoan);
        public const string CanPayLoan = nameof(CanPayLoan);
        public const string Cash = nameof(Cash);

        public const string QuickMoney = nameof(QuickMoney);
        public const string PayLoan = nameof(PayLoan);
        public const string RD = nameof(RD);
    }

    public class Info
    {
        public static void Log(string s)
        {
            Dbg.Log(s);
        }
    }
}
