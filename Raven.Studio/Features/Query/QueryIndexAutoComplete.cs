﻿// -----------------------------------------------------------------------
//  <copyright file="QueryIndexAutoComplete.cs" company="Hibernating Rhinos LTD">
//      Copyright (c) Hibernating Rhinos LTD. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt;
using Raven.Studio.Infrastructure;
using Raven.Studio.Models;

namespace Raven.Studio.Features.Query
{
    public class QueryIndexAutoComplete : NotifyPropertyChangedBase
    {
        private readonly string indexName;
        private readonly Observable<string> query;
        private static readonly Regex FieldsFinderRegex = new Regex(@"(^|\s)?([^\s:]+):", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        private readonly BindableCollection<string> fields = new BindableCollection<string>(x => x);
        private readonly Dictionary<string, List<string>> fieldsTermsDictionary = new Dictionary<string, List<string>>();

        private ICompletionProvider completionProvider;
        public ICompletionProvider CompletionProvider
        {
            get { return completionProvider; }
            set
            {
                completionProvider = value;
                OnPropertyChanged();
            }
        }

        public QueryIndexAutoComplete(string indexName, Observable<string> query, IList<string> fields)
        {
            this.indexName = indexName;
            this.query = query;

            this.fields.Match(fields);

            this.query.PropertyChanged += GetTermsForUsedFields;
            CompletionProvider = new QueryIntelliPromptProvider(fields, fieldsTermsDictionary);
        }

        private void GetTermsForUsedFields(object sender, PropertyChangedEventArgs e)
        {
            var text = ((Observable<string>) sender).Value;
            if (string.IsNullOrEmpty(text))
                return;

            var matches = FieldsFinderRegex.Matches(text);
            foreach (Match match in matches)
            {
                var field = match.Groups[2].Value;
                if (fieldsTermsDictionary.ContainsKey(field))
                    continue;
                var terms = fieldsTermsDictionary[field] = new List<string>();
                GetTermsForField(field, string.Empty, terms);
            }
        }

        private void GetTermsForField(string field, string currentTerm, List<string> terms)
        {
            ApplicationModel.DatabaseCommands.GetTermsAsync(indexName, field, currentTerm, 1024)
                .ContinueOnSuccess(termsFromServer =>
                                       {
                                           foreach (var term in termsFromServer)
                                           {
                                               if (term.IndexOfAny(new[] {' ', '\t'}) == -1)
                                                   terms.Add(term);
                                               else
                                                   terms.Add('"' + term + '"'); // quote the term
                                           }
                                       });
        }
    }
}