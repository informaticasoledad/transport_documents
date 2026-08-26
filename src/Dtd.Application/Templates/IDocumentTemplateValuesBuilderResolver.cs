using System;
using System.Collections.Generic;
using System.Text;

namespace Dtd.Application.Templates
{
    public interface IDocumentTemplateValuesBuilderResolver
    {
        IDocumentTemplateValuesBuilder Resolve(string documentType);
    }
}
