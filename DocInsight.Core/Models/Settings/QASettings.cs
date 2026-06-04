using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocInsight.Core.Models.Settings
{
    public class QASettings
    {
        public const string SectionName = "QASettings";

        public string NoResultsMessage { get; set; } =
            "I don't have enough information to answer this question based on the provided documents";

    }
}

