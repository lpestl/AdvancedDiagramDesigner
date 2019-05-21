using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using ToolboxDesigner.Core;

namespace DiagramDesigner.Functionality
{
    public static class BuilderTypePropertiesOwner
    {
        // Note: From here https://stackoverflow.com/questions/3862226/how-to-dynamically-create-a-class-in-c

        // Note: This a Simple How To Use
        public static void CreateNewObject(List<Property> properties)
        {
            var myType = CompileResultType(properties, "none");
            var myObject = Activator.CreateInstance(myType);
        }

        public static Type CompileResultType(List<Property> properties, string ownerId)
        {
            TypeBuilder tb = GetTypeBuilder(ownerId);
            ConstructorBuilder constructor = tb.DefineDefaultConstructor(
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);

            foreach (var property in properties)
            {
                CreateProperty(tb, property);
            }

            Type objectType = tb.CreateType();
            return objectType;
        }

        private static ulong _revisionVersion;
        private static TypeBuilder GetTypeBuilder(string ownerId)
        {
            var typeSignature = $"TypeProp_id{ownerId}_{_revisionVersion++}";
            var an = new AssemblyName(typeSignature);
            AssemblyBuilder assemblyBuilder =
                AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("DiagramDesigner");
            TypeBuilder tb = moduleBuilder.DefineType(typeSignature,
                TypeAttributes.Public |
                TypeAttributes.Class |
                TypeAttributes.AutoClass |
                TypeAttributes.AnsiClass |
                TypeAttributes.BeforeFieldInit |
                TypeAttributes.AutoLayout,
                null);

            return tb;
        }

        private static Type UpdatePropertyType(Property property)
        {
            Type propType = typeof(string);
            if (property.Type.ToLower().Equals("int32"))
                propType = typeof(Int32);
            else
            if (property.Type.ToLower().Equals("int64"))
                propType = typeof(Int64);
            else
            if (property.Type.ToLower().Equals("bool"))
                propType = typeof(bool);
            else
            if (property.Type.ToLower().Equals("float"))
                propType = typeof(float);
            else
            if (property.Type.ToLower().Equals("double"))
                propType = typeof(double);
            else
            if (property.Type.ToLower().Equals("date") || property.Type.ToLower().Equals("datetime"))
                propType = typeof(DateTime);
            //else
            //if (property.Type.ToLower().Equals("enum"))
            //    propType = typeof(Enum);
            return propType;
        }

        private static void CreateProperty(TypeBuilder tb, Property property)
        {
            var propType = UpdatePropertyType(property);
            //Type editorType = typeof(StringEditorControl);

            FieldBuilder fieldBuilder = tb.DefineField("_" + property.Name, /*Type dynamic property (or type editor view model)*/propType, FieldAttributes.Private);
            PropertyBuilder propertyBuilder =
                tb.DefineProperty(property.Name, PropertyAttributes.HasDefault, /*Type dynamic property (or type editor view model)*/propType, null);

            ConstructorInfo displayCtor = typeof(CategoryAttribute).GetConstructor(new[] { typeof(string) });

            if (displayCtor != null)
            {
                CustomAttributeBuilder displayAttrib = new CustomAttributeBuilder(displayCtor,
                    new object[] { property.CatalogName });

                propertyBuilder.SetCustomAttribute(displayAttrib);
            }

            //ConstructorInfo editorCtor =
            //    typeof(TelerikGrid.EditorAttribute).GetConstructor(new[] { typeof(Type) });

            //if (editorCtor != null)
            //{
            //    // todo: need all custom view editors
            //    CustomAttributeBuilder editorAttrib = new CustomAttributeBuilder(editorCtor,
            //        new object[] { editorType });

            //    propertyBuilder.SetCustomAttribute(editorAttrib);
            //}

            MethodBuilder getPropMthdBldr = tb.DefineMethod("get_" + property.Name,
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, /*Type dynamic property (or type editor view model)*/propType,
                Type.EmptyTypes);
            ILGenerator getIl = getPropMthdBldr.GetILGenerator();

            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Ldfld, fieldBuilder);
            getIl.Emit(OpCodes.Ret);

            MethodBuilder setPropMthdBldr =
                tb.DefineMethod("set_" + property.Name,
                    MethodAttributes.Public |
                    MethodAttributes.SpecialName |
                    MethodAttributes.HideBySig,
                    null, new[] { /*Type dynamic property (or type editor view model)*/propType });

            ILGenerator setIl = setPropMthdBldr.GetILGenerator();
            Label modifyProperty = setIl.DefineLabel();
            Label exitSet = setIl.DefineLabel();

            setIl.MarkLabel(modifyProperty);
            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Ldarg_1);
            setIl.Emit(OpCodes.Stfld, fieldBuilder);

            setIl.Emit(OpCodes.Nop);
            setIl.MarkLabel(exitSet);
            setIl.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getPropMthdBldr);
            propertyBuilder.SetSetMethod(setPropMthdBldr);
        }
    }
}
