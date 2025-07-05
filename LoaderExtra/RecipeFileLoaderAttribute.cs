using System;

namespace RecipaediaEX {
    [AttributeUsage(AttributeTargets.Class)]
    public class RecipeFileLoaderAttribute : Attribute {
        public string TargetModPackageName { get; protected set; }

        public RecipeFileLoaderAttribute(string targetModPackageName) {
            TargetModPackageName = targetModPackageName;
        }
    }
}