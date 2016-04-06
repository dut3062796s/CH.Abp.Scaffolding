using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using EnvDTE;
using CH.Abp.Scaffolding.UI;
using CH.Abp.Scaffolding.Utils;
using Microsoft.AspNet.Scaffolding;

namespace CH.Abp.Scaffolding.Scaffolders
{
    // 此类包含基架生成的所有步骤:
    // 1) ShowUIAndValidate() - 显示一个Visual Studio的对话框用于设置生成参数
    // 2) Validate() - 确认提取的Model   validates the model collected from the dialog
    // 3) GenerateCode() - 根据模板生成代码文件 if all goes well, generates the scaffolding output from the templates
    public class ModuleScaffolder : CodeGenerator
    {

        private GeneratorModuleViewModel _moduleViewModel;

        internal ModuleScaffolder(CodeGenerationContext context, CodeGeneratorInformation information)
            : base(context, information)
        {

        }

        public override bool ShowUIAndValidate()
        {
            _moduleViewModel = new GeneratorModuleViewModel(Context);

            GeneratorModuleDialog window = new GeneratorModuleDialog(_moduleViewModel);
            bool? isOk = window.ShowModal();

            if (isOk == true)
            {
                Validate();
            }
            return (isOk == true);
        }


        // Validates the model returned by the Visual Studio dialog.
        // We always force a Visual Studio build so we have a model
        private void Validate()
        {
            CodeType modelType = _moduleViewModel.ModelType.CodeType;

            if (modelType == null)
            {
                throw new InvalidOperationException("请选择一个有效的实体类。");
            }

            var visualStudioUtils = new VisualStudioUtils();
            visualStudioUtils.BuildProject(Context.ActiveProject);


            Type reflectedModelType = GetReflectionType(modelType.FullName);
            if (reflectedModelType == null)
            {
                throw new InvalidOperationException("不能加载的实体类型。如果项目没有编译，请编译后重试。");
            }
        }

        public override void GenerateCode()
        {
            if (_moduleViewModel == null)
            {
                throw new InvalidOperationException("需要先调用ShowUIAndValidate方法。");
            }

            Cursor currentCursor = Mouse.OverrideCursor;
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                generateCode();
            }
            finally
            {
                Mouse.OverrideCursor = currentCursor;
            }
        }


        private void generateCode()
        {
            var project = Context.ActiveProject;
            var entity = _moduleViewModel.ModelType.CodeType;
            var entityName = entity.Name;
            var projectNamespace = project.GetDefaultNamespace();
            var projectName = project.Name.Split('.')[1];
            var entityNamespace = entity.Namespace.FullName;
            var moduleNamespace = getModuleNamespace(entityNamespace);
            var moduleName = getModuleName(moduleNamespace);
            var functionName = _moduleViewModel.FunctionName;
            var functionFolderName = _moduleViewModel.FunctionFolderName;
            var isDisplayOrderable = IsDisplayOrderable(entity);
            var overwrite = _moduleViewModel.OverwriteFiles;

            Dictionary<string, object> templateParams = new Dictionary<string, object>(){
                {"ProjectNamespace", projectNamespace}
                , {"EntityNamespace", entityNamespace}
                , {"ModuleNamespace", moduleNamespace}
                , {"ModuleName", moduleName}
                , {"EntityName", entityName}
                , {"ProjectName", projectName}
                , {"FunctionName", functionName}
                , {"FunctionFolderName", functionFolderName}
                , {"DtoMetaTable", _moduleViewModel.DtoClassMetadataViewModel.DataModel}
                , {"AllowBatchDelete", _moduleViewModel.AllowBatchDelete}
                , {"IsDisplayOrderable", isDisplayOrderable}
            };

            var templates = new[] {
                @"Module\Application\{FunctionFolderName}\Dto\Create{Entity}Dto"
                , @"Module\Application\{FunctionFolderName}\Dto\Get{Entity}ListDto"
                , @"Module\Application\{FunctionFolderName}\Dto\Update{Entity}Dto"
                , @"Module\Application\{FunctionFolderName}\I{Entity}AppService"
                , @"Module\Application\{FunctionFolderName}\{Entity}AppService"
                , @"Module\Application\{FunctionFolderName}\{Entity}Constants"
                , @"Module\Domain\{FunctionFolderName}\Repositories\I{Entity}Repository"
                , @"Module\Repository\{FunctionFolderName}\Repositories\{Entity}Repository"
            };
        
            foreach (var template in templates)
            {
                string outputPath = Path.Combine(@"_GeneratedCode\" + moduleName, template.Replace("{Entity}", entityName));
                if (string.IsNullOrWhiteSpace(functionFolderName))
                {
                    outputPath = outputPath.Replace(@"\{FunctionFolderName}", "");
                }
                else
                {
                    outputPath = outputPath.Replace("{FunctionFolderName}", functionFolderName);
                }
                //WriteLog("outputPath:" + outputPath);
                //WriteLog("templatePath:" + templatePath);
                AddFileFromTemplate(project, outputPath, template, templateParams, !overwrite);
            }

        }

        private void WriteLog(string str)
        {
            File.AppendAllText("D:\\Scaffolder.log.txt", str + "\r\n");
        }

        private bool IsDisplayOrderable(CodeType entity)
        {
            return entity.IsDerivedType("Abp.Domain.Entities.IDisplayOrderable");
        }

        private string getModuleNamespace(string entityNamespace)
        {
            var list = entityNamespace.Split('.').ToList();
            return string.Join(".", list); 
        }

        private string getModuleName(string moduleNamespace)
        {
            return moduleNamespace.Split('.').Last();
        }


        #region function library


        // Called to ensure that the project was compiled successfully
        private Type GetReflectionType(string typeName)
        {
            return GetService<IReflectedTypesService>().GetType(Context.ActiveProject, typeName);
        }

        private TService GetService<TService>() where TService : class
        {
            return (TService)ServiceProvider.GetService(typeof(TService));
        }


        // Returns the relative path of the folder selected in Visual Studio or an empty 
        // string if no folder is selected.
        protected string GetSelectionRelativePath()
        {
            return Context.ActiveProjectItem == null ? String.Empty : ProjectItemUtils.GetProjectRelativePath(Context.ActiveProjectItem);
        }

        #endregion


    }
}
