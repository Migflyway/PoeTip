using System;


namespace tools.Dev
{
    public interface IPlugin
    {
        PluginInfoAttribute PluginInfo { get; set; }

        TextResult ProcessClipValue(string ClipboardText, bool NameOnly);
        //bool FilterClipValue(string ClipboardText);
    }

    public class TextResult
    {
        public string textTip { get; set; }
        public string textClip { get; set; }
    }

    public class PluginInfoAttribute : Attribute
    {

        public PluginInfoAttribute() { }

        public PluginInfoAttribute(string name, string version, string author)
        {
            _Name = name;
            _Version = version;
            _Author = author;
        }

        public string Name { get{return _Name;} }

        public string Version { get{return _Version;} }

        public string Author{ get{return _Author;} }


        private string _Name = "";
        private string _Version = "";
        private string _Author = "";

    }

}
