using System.Runtime.ConstrainedExecution;
using System.Text.RegularExpressions;

namespace VolleyballRallyManager.Lib.Models
{
    public enum GroupGenerationStrategy
    {

        /// <summary>
        /// Groups are generated based on the desired number of teams per group
        /// </summary>
        TeamsPerGroup,

        /// <summary>
        /// Groups are generated based on the desired number of groups in the round
        /// </summary>
        GroupsInRound,

        /// <summary>
        /// Groups are not used or generated manually
        /// </summary>
        NoGroup


    }
}