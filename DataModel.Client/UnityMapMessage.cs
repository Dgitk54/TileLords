using DataModel.Common;
using System.Collections.Generic;

namespace DataModel.Client
{
    /// <summary>
    /// Small Message for Unity updating current map state.
    /// </summary>
    public class UnityMapMessage
    {
        public PlusCode ClientLocation { get; set; }
        public List<MiniTile> VisibleMap { get; set; }
    }
}
