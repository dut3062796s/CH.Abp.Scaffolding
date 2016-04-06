using EnvDTE;
using Microsoft.AspNet.Scaffolding;
using Microsoft.AspNet.Scaffolding.NuGet;
using Microsoft.AspNet.Scaffolding.EntityFramework.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using CH.Abp.Scaffolding.UI;
using System.Windows.Media;
using System.Drawing;
using System.Windows.Interop;
using System.Windows;
using System.Windows.Media.Imaging;
using CH.Abp.Scaffolding.Scaffolders;

namespace CH.Abp.Scaffolding
{    

    [Export(typeof(CodeGeneratorFactory))]
    public class ScaffolderFactory : CodeGeneratorFactory
    {
        public ScaffolderFactory()
            : base(CreateCodeGeneratorInformation())
        {

        }

        public override ICodeGenerator CreateInstance(CodeGenerationContext context)
        {
            return new Scaffolder(context, Information);
        }

        // We support CSharp WAPs targetting at least .Net Framework 4.5 or above.
        // We DON'T currently support VB
        public override bool IsSupported(CodeGenerationContext codeGenerationContext)
        {
            if (ProjectLanguage.CSharp.Equals(codeGenerationContext.ActiveProject.GetCodeLanguage()))
            {
                FrameworkName targetFramework = codeGenerationContext.ActiveProject.GetTargetFramework();
                return (targetFramework != null) &&
                        String.Equals(".NetFramework", targetFramework.Identifier, StringComparison.OrdinalIgnoreCase) &&
                        targetFramework.Version >= new Version(4, 5);
            }

            return false;
        }

        private static CodeGeneratorInformation CreateCodeGeneratorInformation()
        {
            return new CodeGeneratorInformation(
                displayName: "[CH.Abp.Scaffolding]项目文件",
                description: "通过实体类，生成相应模块的代码",
                author: "CH",
                version: new Version(0, 1, 0, 0),
                id: "ABP_Scaffolding",
                icon: ToImageSource(Resources.Application),
                gestures: new[] { "ABP" },
                categories: new[] { "ABP", Categories.Common, Categories.Other }
            );
        }

        /// <summary>
        /// Helper method to convert Icon to Imagesource.
        /// </summary>
        /// <param name="icon">Icon</param>
        /// <returns>Imagesource</returns>
        public static ImageSource ToImageSource(Icon icon)
        {
            ImageSource imageSource = Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            return imageSource;
        }
    }
}
