﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Infrastructure;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Apps.Models;

public sealed class AppLanguageDto : Resource
{
    /// <summary>
    /// The iso code of the language.
    /// </summary>
    public string Iso2Code { get; set; }

    /// <summary>
    /// The english name of the language.
    /// </summary>
    public string EnglishName { get; set; }

    /// <summary>
    /// The fallback languages.
    /// </summary>
    public Language[] Fallback { get; set; }

    /// <summary>
    /// Indicates if the language is the master language.
    /// </summary>
    public bool IsMaster { get; set; }

    /// <summary>
    /// Indicates if the language is optional.
    /// </summary>
    public bool IsOptional { get; set; }

    public static AppLanguageDto FromDomain(Language language, LanguageConfig config, LanguagesConfig languages)
    {
        var result = new AppLanguageDto
        {
            EnglishName = language.EnglishName,
            IsMaster = languages.IsMaster(language),
            IsOptional = languages.IsOptional(language),
            Iso2Code = language.Iso2Code,
            Fallback = config.Fallbacks.ToArray(),
        };

        return result;
    }

    public AppLanguageDto CreateLinks(Resources resources, App app)
    {
        var values = new { app = resources.App, language = Iso2Code };

        if (!IsMaster)
        {
            if (resources.CanUpdateLanguage)
            {
                AddPutLink("update",
                    resources.Url<AppLanguagesController>(x => nameof(x.PutLanguage), values));
            }

            if (resources.CanDeleteLanguage && app.Languages.Values.Count > 1)
            {
                AddDeleteLink("delete",
                    resources.Url<AppLanguagesController>(x => nameof(x.DeleteLanguage), values));
            }
        }

        return this;
    }
}
