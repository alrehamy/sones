using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sones.GraphDB.TypeSystem;
using sones.GraphDS.GraphDSRemoteClient.sonesGraphDSRemoteAPI;
using sones.GraphDS.GraphDSRemoteClient.TypeManagement;

namespace sones.GraphDS.GraphDSRemoteClient.GraphElements
{
    internal class RemoteVertexType : ARemoteBaseType, IVertexType
    {
        #region Data

        private bool _IsAbstract;

        #endregion

        #region Constructor

        internal RemoteVertexType(ServiceVertexType myVertexType, IServiceToken myServiceToken) : base(myVertexType, myServiceToken)
        {
            _IsAbstract = myVertexType.IsAbstract;
        }

        #endregion


        #region ARemoteBaseType

        protected override ARemoteBaseType RetrieveParentType()
        {
            return HasParentType ? new RemoteVertexType(_ServiceToken.VertexTypeService.ParentVertexType(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexType(this._Name)), _ServiceToken) : null;
        }

        protected override IEnumerable<ARemoteBaseType> RetrieveChildrenTypes()
        {
            if (!HasChildTypes)
                return Enumerable.Empty<ARemoteBaseType>();

            var vertices = _ServiceToken.VertexTypeService.ChildrenVertexTypes(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexType(this));

            return vertices.Select(vertex => new RemoteVertexType(vertex, _ServiceToken)).ToArray();
        }

        protected override IDictionary<string, IAttributeDefinition> RetrieveAttributes()
        {
            throw new NotImplementedException();
        }

        #endregion


        #region IVertexType

        public bool IsAbstract
        {
            get { return _IsAbstract; }
        }

        public IEnumerable<IVertexType> GetDescendantVertexTypes()
        {
            return _ServiceToken.VertexTypeService.GetDescendantVertexTypes(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexType(this))
                .Select(x => (IVertexType)new RemoteVertexType(x, _ServiceToken));
        }

        public IEnumerable<IVertexType> GetDescendantVertexTypesAndSelf()
        {
            return _ServiceToken.VertexTypeService.GetDescendantVertexTypesAndSelf(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexType(this))
                .Select(x => (IVertexType)new RemoteVertexType(x, _ServiceToken));
        }

        public IEnumerable<IVertexType> GetAncestorVertexTypes()
        {
            return _ServiceToken.VertexTypeService.GetAncestorVertexTypes(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexType(this))
                .Select(x => (IVertexType)new RemoteVertexType(x, _ServiceToken));
        }

        public IEnumerable<IVertexType> GetAncestorVertexTypesAndSelf()
        {
            return _ServiceToken.VertexTypeService.GetAncestorVertexTypesAndSelf(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexType(this))
                .Select(x => (IVertexType)new RemoteVertexType(x, _ServiceToken));
        }

        public IEnumerable<IVertexType> GetKinsmenVertexTypes()
        {
            return _ServiceToken.VertexTypeService.GetKinsmenVertexTypes(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexType(this))
                .Select(x => (IVertexType)new RemoteVertexType(x, _ServiceToken));
        }

        public IEnumerable<IVertexType> GetKinsmenVertexTypesAndSelf()
        {
            return _ServiceToken.VertexTypeService.GetKinsmenVertexTypesAndSelf(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexType(this))
                .Select(x => (IVertexType)new RemoteVertexType(x, _ServiceToken));
        }

        public IEnumerable<IVertexType> ChildrenVertexTypes
        {
            get { return RetrieveChildrenTypes().Cast<IVertexType>(); }
        }

        public IVertexType ParentVertexType
        {
            get { return (IVertexType)RetrieveParentType(); }
        }

        public bool HasBinaryProperty(string myEdgeName)
        {
            return _ServiceToken.VertexTypeService.HasBinaryProperty(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexType(this), myEdgeName);
        }

        public IBinaryPropertyDefinition GetBinaryPropertyDefinition(string myEdgeName)
        {
            return new RemoteBinaryPropertyDefinition(
                _ServiceToken.VertexTypeService.GetBinaryPropertyDefinition(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexType(this), myEdgeName),
                _ServiceToken);
        }

        public bool HasBinaryProperties(bool myIncludeAncestorDefinitions)
        {
            return _ServiceToken.VertexTypeService.HasBinaryProperties(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexType(this), myIncludeAncestorDefinitions);
        }

        public IEnumerable<IBinaryPropertyDefinition> GetBinaryProperties(bool myIncludeAncestorDefinitions)
        {
            return _ServiceToken.VertexTypeService.GetBinaryPropertyDefinitions(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexType(this), myIncludeAncestorDefinitions)
                .Select(x => (IBinaryPropertyDefinition)new RemoteBinaryPropertyDefinition(x, _ServiceToken));
        }

        public bool HasIncomingEdge(string myEdgeName)
        {
            return _ServiceToken.VertexTypeService.HasIncomingEdge(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexType(this), myEdgeName);
        }

        public IIncomingEdgeDefinition GetIncomingEdgeDefinition(string myEdgeName)
        {
            return new RemoteIncomingEdgeDefinition(
                _ServiceToken.VertexTypeService.GetIncomingEdgeDefinition(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexType(this), myEdgeName),
                _ServiceToken);
        }

        public bool HasIncomingEdges(bool myIncludeAncestorDefinitions)
        {
            return _ServiceToken.VertexTypeService.HasIncomingEdges(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexType(this), myIncludeAncestorDefinitions);
        }

        public IEnumerable<IIncomingEdgeDefinition> GetIncomingEdgeDefinitions(bool myIncludeAncestorDefinitions)
        {
            return _ServiceToken.VertexTypeService.GetIncomingEdgeDefinitions(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexType(this), myIncludeAncestorDefinitions)
                .Select(x => (IIncomingEdgeDefinition)new RemoteIncomingEdgeDefinition(x, _ServiceToken));
        }

        public bool HasOutgoingEdge(string myEdgeName)
        {
            return _ServiceToken.VertexTypeService.HasOutgoingEdgeByVertexType(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexType(this), myEdgeName);
        }

        public IOutgoingEdgeDefinition GetOutgoingEdgeDefinition(string myEdgeName)
        {
            return new RemoteOutgoingEdgeDefinition(
                _ServiceToken.VertexTypeService.GetOutgoingEdgeDefinitionByVertexType(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexType(this), myEdgeName),
                _ServiceToken);
        }

        public bool HasOutgoingEdges(bool myIncludeAncestorDefinitions)
        {
            return _ServiceToken.VertexTypeService.HasOutgoingEdgesByVertexType(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexType(this), myIncludeAncestorDefinitions);
        }

        public IEnumerable<IOutgoingEdgeDefinition> GetOutgoingEdgeDefinitions(bool myIncludeAncestorDefinitions)
        {
            return _ServiceToken.VertexTypeService.GetOutgoingEdgeDefinitionsByVertexType(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexType(this), myIncludeAncestorDefinitions)
                .Select(x => (IOutgoingEdgeDefinition)new RemoteOutgoingEdgeDefinition(x, _ServiceToken));
        }

        public bool HasUniqueDefinitions(bool myIncludeAncestorDefinitions)
        {
            return _ServiceToken.VertexTypeService.HasUniqueDefinitions(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexType(this), myIncludeAncestorDefinitions);
        }

        public IEnumerable<IUniqueDefinition> GetUniqueDefinitions(bool myIncludeAncestorDefinitions)
        {
            return _ServiceToken.VertexTypeService.GetUniqueDefinitions(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexType(this), myIncludeAncestorDefinitions)
                .Select(x => (IUniqueDefinition)new RemoteUniqueDefinition(x, _ServiceToken));
        }

        public bool HasIndexDefinitions(bool myIncludeAncestorDefinitions)
        {
            return _ServiceToken.VertexTypeService.HasIndexDefinitions(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexType(this), myIncludeAncestorDefinitions);
        }

        public IEnumerable<IIndexDefinition> GetIndexDefinitions(bool myIncludeAncestorDefinitions)
        {
            return _ServiceToken.VertexTypeService.GetIndexDefinitions(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexType(this), myIncludeAncestorDefinitions)
                .Select(x => (IIndexDefinition)new RemoteIndexDefinition(x, _ServiceToken));
        }

        #endregion


        #region IBaseType

        public override bool IsSealed
        {
            get
            {
                return _ServiceToken.VertexTypeService.IsSealedByVertexType(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexType(this));
            }
        }

        public override bool HasParentType
        {
            get
            {
                return _ServiceToken.VertexTypeService.HasParentTypeByVertexType(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexType(this));
            }
        }

        public override bool HasChildTypes
        {
            get
            {
                return _ServiceToken.VertexTypeService.HasChildTypeByVertexType(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexType(this));
            }
        }

        public override bool IsAncestor(IBaseType myOtherType)
        {
            return (myOtherType is IVertexType) ? _ServiceToken.VertexTypeService.IsAncestorByVertexType(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexType(this), new ServiceVertexType((IVertexType)myOtherType)) : false;
        }

        public override bool IsAncestorOrSelf(IBaseType myOtherType)
        {
            return (myOtherType is IVertexType) ? _ServiceToken.VertexTypeService.IsAncestorOrSelfByVertexType(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexType(this), new ServiceVertexType((IVertexType)myOtherType)) : false;
        }

        public override bool IsDescendant(IBaseType myOtherType)
        {
            return (myOtherType is IVertexType) ? _ServiceToken.VertexTypeService.IsDescendantByVertexType(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexType(this), new ServiceVertexType((IVertexType)myOtherType)) : false;
        }

        public override bool IsDescendantOrSelf(IBaseType myOtherType)
        {
            return (myOtherType is IVertexType) ? _ServiceToken.VertexTypeService.IsDescendantOrSelfByVertexType(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexType(this), new ServiceVertexType((IVertexType)myOtherType)) : false;
        }

        public override IEnumerable<IBaseType> GetDescendantTypes()
        {
            return GetDescendantVertexTypes().Select(_ => (IBaseType)_);
        }

        public override IEnumerable<IBaseType> GetDescendantTypesAndSelf()
        {
            return GetDescendantVertexTypesAndSelf().Select(_ => (IBaseType)_);
        }

        public override IEnumerable<IBaseType> GetAncestorTypes()
        {
            return GetAncestorVertexTypes().Select(_ => (IBaseType)_);
        }

        public override IEnumerable<IBaseType> GetAncestorTypesAndSelf()
        {
            return GetAncestorVertexTypesAndSelf().Select(_ => (IBaseType)_);
        }

        public override IEnumerable<IBaseType> GetKinsmenTypes()
        {
            return GetKinsmenVertexTypes().Select(_ => (IBaseType)_);
        }

        public override IEnumerable<IBaseType> GetKinsmenTypesAndSelf()
        {
            return GetKinsmenVertexTypesAndSelf().Select(_ => (IBaseType)_);
        }

        public override IEnumerable<IBaseType> ChildrenTypes
        {
            get { return RetrieveChildrenTypes().Select(_ => (IBaseType)_); }
        }

        public override IBaseType ParentType
        {
            get { return RetrieveParentType(); }
        }

        public override bool HasAttribute(string myAttributeName)
        {
            return _ServiceToken.VertexTypeService.HasAttributeByVertexType(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexType(this), myAttributeName);
        }

        public override IAttributeDefinition GetAttributeDefinition(string myAttributeName)
        {
            return ConvertHelper.ToAttributeDefinition(
                _ServiceToken.VertexTypeService.GetAttributeDefinitionByVertexType(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexType(this), myAttributeName),
                _ServiceToken);
        }

        public override IAttributeDefinition GetAttributeDefinition(long myAttributeID)
        {
            return ConvertHelper.ToAttributeDefinition(
                _ServiceToken.VertexTypeService.GetAttributeDefinitionByIDByVertexType(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexType(this), myAttributeID),
                _ServiceToken);
        }

        public override bool HasAttributes(bool myIncludeAncestorDefinitions)
        {
            return _ServiceToken.VertexTypeService.HasAttributesByVertexType(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexType(this), myIncludeAncestorDefinitions);
        }

        public override IEnumerable<IAttributeDefinition> GetAttributeDefinitions(bool myIncludeAncestorDefinitions)
        {
            return _ServiceToken.VertexTypeService.GetAttributeDefinitionsByVertexType(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexType(this), myIncludeAncestorDefinitions)
                .Select(x => ConvertHelper.ToAttributeDefinition(x, _ServiceToken));
        }

        public override bool HasProperty(string myAttributeName)
        {
            return _ServiceToken.VertexTypeService.HasPropertyByVertexType(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexType(this), myAttributeName);
        }

        public override IPropertyDefinition GetPropertyDefinition(string myPropertyName)
        {
            var bla = _ServiceToken.VertexTypeService.GetPropertyDefinitionByVertexType(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, this.Name, myPropertyName);
            return new RemotePropertyDefinition(
                bla,
                _ServiceToken);
        }

        public override IPropertyDefinition GetPropertyDefinition(long myPropertyID)
        {
            return new RemotePropertyDefinition(
                _ServiceToken.VertexTypeService.GetPropertyDefinitionByIDByVertexType(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, this.Name, myPropertyID),
                _ServiceToken);
        }

        public override bool HasProperties(bool myIncludeAncestorDefinitions)
        {
            return _ServiceToken.VertexTypeService.HasPropertiesByVertexType(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexType(this), myIncludeAncestorDefinitions);
        }

        public override IEnumerable<IPropertyDefinition> GetPropertyDefinitions(bool myIncludeAncestorDefinitions)
        {
            return _ServiceToken.VertexTypeService.GetPropertyDefinitionsByVertexType(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, this.Name, myIncludeAncestorDefinitions)
                .Select(x => (IPropertyDefinition)new RemotePropertyDefinition(x, _ServiceToken));
        }

        public override IEnumerable<IPropertyDefinition> GetPropertyDefinitions(IEnumerable<string> myPropertyNames)
        {
            return _ServiceToken.VertexTypeService.GetPropertyDefinitionsByNameListByVertexType(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, this.Name, myPropertyNames.ToArray())
                .Select(x => (IPropertyDefinition)new RemotePropertyDefinition(x, _ServiceToken));
        }

        #endregion
    }
}
