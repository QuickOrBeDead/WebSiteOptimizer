﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Labo.WebSiteOptimizer.Caching;
using Labo.WebSiteOptimizer.ResourceManagement.Exceptions;
using Labo.WebSiteOptimizer.ResourceManagement.Resolver;

namespace Labo.WebSiteOptimizer.ResourceManagement.Configuration
{
    public sealed class ResourceXmlConfigurationProvider : IResourceConfigurationProvider
    {
        private readonly ICacheProvider m_CacheProvider;
        private readonly string m_XmlConfigurationPath;
        private static readonly XmlSerializer s_XmlSerializer = new XmlSerializer();

        private WebResources WebResources
        {
            get
            {
                return m_CacheProvider.GetOrAdd("ResourceManagement:Labo.WebResources.Configuration", () => LoadWebResourcesConfig(m_XmlConfigurationPath), TimeSpan.FromHours(1), () => new List<string> { m_XmlConfigurationPath });
            }
        }

        public ResourceXmlConfigurationProvider(ICacheProvider cacheProvider, IVirtualPathResolver virtualPathResolver)
            : this(cacheProvider, virtualPathResolver.Resolve("~/App_Data/WebResources.xml"))
        {
        }

        public ResourceXmlConfigurationProvider(ICacheProvider cacheProvider, string configurationPath)
        {
            m_CacheProvider = cacheProvider;
            m_XmlConfigurationPath = configurationPath;
        }

        public ResourceElementGroup GetResourceElementGroup(ResourceType resourceType, string resourceGroupName)
        {
            ResourceElementGroup resourceElementGroup;
            switch (resourceType)
            {
                case ResourceType.Js:
                    resourceElementGroup = WebResources.JavascriptResources.GetResourceGroupByName(resourceGroupName);
                    resourceElementGroup.ResourceType = resourceType;
                    break;
                case ResourceType.Css:
                    resourceElementGroup = WebResources.CssResources.GetResourceGroupByName(resourceGroupName);
                    resourceElementGroup.ResourceType = resourceType;
                    break;
                default:
                    throw new ResourceConfigurationException(String.Format(CultureInfo.CurrentCulture, "resource type '{0}' not supported", resourceType));
            }
            return resourceElementGroup;
        }

        private static WebResources LoadWebResourcesConfig(string xmlPath)
        {
            string xml = File.ReadAllText(xmlPath);
            return s_XmlSerializer.Deserialize<WebResources>(xml);
        }
    }
}