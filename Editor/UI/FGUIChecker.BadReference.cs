using System.Xml;

namespace GameFrame.Editor
{
    public partial class FGUIChecker
    {
        public static string FindXmlAttr(XmlNode node, string attrName)
        {
            for (int i = 0; i < node.Attributes.Count; i++)
            {
                if (node.Attributes[i].Name == attrName)
                {
                    return node.Attributes[i].Value;
                }
            }

            return null;
        }

        string GetPackageID(string name)
        {
            packageNameToID.TryGetValue(name, out var ret);
            return ret;
        }

        string GetPackageName(string id)
        {
            foreach (var (k, v) in packageNameToID)
            {
                if (v == id) return k;
            }

            return null;
        }
    }
}