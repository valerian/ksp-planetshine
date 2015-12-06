using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlanetShine
{
    public static class GameDatabaseExtensions
    {
        public static ConfigNode FindConfigNode(this GameDatabase gameDatabase, string typeName, string fieldName, string value)
        {
            return Array.Find(gameDatabase.GetConfigNodes(typeName),
                node => (node.HasValue(fieldName) ? (value == node.GetValue(fieldName)) : false));
        }
    }
}
