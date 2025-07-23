using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Newtonsoft.Json;
using Sitecore.Abstractions;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.LayoutService.Configuration;
using Sitecore.LayoutService.ItemRendering.ContentsResolvers;
using Sitecore.LayoutService.Serialization.Pipelines.GetFieldSerializer;
using Sitecore.Links;
using Sitecore.Links.UrlBuilders;
using Sitecore.Mvc.Presentation;

namespace Sitecore.Demo.Edge.Website.SitecoreExtensions.LayoutService
{
    public class MainNavigationRenderingContentsResolver : RenderingContentsResolver
    {
        protected readonly IGetFieldSerializerPipeline getFieldSerializerPipeline;
        protected readonly BaseLinkManager linkManager;
        protected readonly BaseMediaManager mediaManager;
        protected readonly BaseLog log;

        public MainNavigationRenderingContentsResolver(IGetFieldSerializerPipeline getFieldSerializerPipeline, BaseLinkManager linkManager, BaseLog log, BaseMediaManager mediaManager)
        {
            this.getFieldSerializerPipeline = getFieldSerializerPipeline;
            this.linkManager = linkManager;
            this.mediaManager = mediaManager;
            this.log = log;
        }

        public override object ResolveContents(Rendering rendering, IRenderingConfiguration renderingConfig)
        {
            var currentItem = Context.Item;

            var configItem = GetConfigItem(currentItem);
            var rootItem = GetRootItem(currentItem);

            Assert.IsNotNull(configItem, nameof(configItem));
            Assert.IsNotNull(rootItem, nameof(rootItem));

            ImageField logoField = (ImageField)configItem.Fields["HeaderLogo"];
            var imageUrl = this.mediaManager.GetMediaUrl(logoField.MediaItem);
            var altText = logoField.Alt;

            var result = new
            {
                data = new
                {
                    item = new
                    {
                        id = configItem.ID.ToString(),
                        path = configItem.Paths.FullPath,
                        headerLogo = new
                        {
                            jsonValue = new
                            {
                                value = new
                                {
                                    src = imageUrl,
                                    alt = altText
                                }
                            },
                            alt = altText
                        }
                    },
                    links = new
                    {
                        displayName = "Main navigation",
                        children = new
                        {
                            results = rootItem.Children.InnerChildren.Where(x => x["ShowInMainNavigation"] == "1").Select(x => new
                            {
                                displayName = !string.IsNullOrWhiteSpace(x["NavigationTitle"]) ? x["NavigationTitle"] : x.DisplayName,
                                field = new
                                {
                                    jsonValue = new
                                    {
                                        value = new
                                        {
                                            href = this.linkManager.GetItemUrl(x, new ItemUrlBuilderOptions { AlwaysIncludeServerUrl = false }),
                                            id = x.ID,
                                            linktype = "internal"
                                        }
                                    }
                                }
                            })
                        }
                    }
                }
            };

            return result;
        }

        private Item GetConfigItem(Item currentItem)
        {
            var configId = Parameters["Config"];
            if (configId != null)
            {
                return currentItem.Database.GetItem(configId);
            }

            return null;
        }

        private Item GetRootItem(Item currentItem)
        {
            var rootId = Parameters["Root"];
            if (rootId != null)
            {
                return currentItem.Database.GetItem(rootId);
            }

            return null;
        }
    }
}
