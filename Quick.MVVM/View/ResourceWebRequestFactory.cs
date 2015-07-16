using Quick.MVVM.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;

namespace Quick.MVVM.View
{
    public class ResourceWebRequest : WebRequest
    {
        private Uri uri;
        public ResourceWebRequest(Uri uri)
        {
            this.uri = uri;
        }

        public override WebResponse GetResponse()
        {
            //resource://{0}/{1}
            Assembly assembly = Assembly.Load(uri.Host);
            String resourceName = uri.LocalPath;
            while (resourceName.StartsWith("/"))
                resourceName = resourceName.Substring(1);
            Uri forwardUri = null;
            foreach (ViewManager viewManager in ViewManager.AllViewManagerHashSet)
            {
                forwardUri = viewManager.GetResourceUri(assembly, resourceName);
                if (forwardUri != null)
                    break;
            }
            if (forwardUri == null)
                return null;
            return WebRequest.Create(forwardUri).GetResponse();
        }
    }

    public class ResourceWebRequestFactory : IWebRequestCreate
    {
        public WebRequest Create(Uri uri)
        {
            return new ResourceWebRequest(uri);
        }
    }
}
