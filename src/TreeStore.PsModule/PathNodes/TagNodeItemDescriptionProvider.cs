//using System;
//using System.ComponentModel;

//namespace TreeStore.PsModule.PathNodes
//{
//    public class TagNodeItemDescriptionProvider : TypeDescriptionProvider
//    {
//        private static readonly TypeDescriptionProvider parentProvider;
//        private readonly TypeDescriptionProvider provider;

//        static TagNodeItemDescriptionProvider()
//        {
//            parentProvider = TypeDescriptor.GetProvider(typeof(TagNode.Item));
//        }

//        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
//        {
//            if (objectType.Equals(typeof(TagNode.Item)))
//                return new TagNodeTypeDescritor(parentProvider.GetTypeDescriptor();
//            else return base.GetTypeDescriptor(objectType, instance);
//        }
//    }

//    public class TagNodeTypeDescritor : CustomTypeDescriptor
//    {
//        private readonly TagNodeItemDescriptionProvider provider;

//        public TagNodeTypeDescritor(TagNodeItemDescriptionProvider provider)
//        {
//            this.provider = provider;
//        }
//    }
//}