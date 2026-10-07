// Mirrors the implicit usings the Storefront.Client project gets from its SDK. Kept explicit
// (ImplicitUsings is off) so the Web SDK's extra namespaces — e.g. Microsoft.AspNetCore.Http,
// whose static `Results` class — don't leak into the linked client code.
global using System;
global using System.Collections.Generic;
global using System.IO;
global using System.Linq;
global using System.Net.Http;
global using System.Threading;
global using System.Threading.Tasks;
