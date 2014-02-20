namespace Sitecore.FakeDb
{
  using System;
  using System.Collections;
  using System.Linq;
  using Sitecore.Configuration;
  using Sitecore.Data;
  using Sitecore.Data.Items;
  using Sitecore.Data.Managers;
  using Sitecore.Diagnostics;
  using Sitecore.FakeDb.Data;
  using Sitecore.FakeDb.Data.Engines;
  using Sitecore.FakeDb.Security.AccessControl;
  using Sitecore.Globalization;
  using Sitecore.Security.AccessControl;

  public class Db : IDisposable, IEnumerable
  {
    private static readonly ID DefaultItemRoot = ItemIDs.ContentRoot;

    private readonly Database database;

    public Db()
      : this("master")
    {
    }

    public Db(string databaseName)
    {
      this.database = Database.GetDatabase(databaseName);

      // TODO:[High] Should not be here
      ((FakeAuthorizationProvider)AuthorizationManager.Provider).DataStorage = this.DataStorage;
    }

    public Database Database
    {
      get { return this.database; }
    }

    private DataStorage DataStorage
    {
      get { return this.Database.GetDataStorage(); }
    }

    public IEnumerator GetEnumerator()
    {
      throw new NotImplementedException();
    }

    public void Add(DbItem item)
    {
      Assert.ArgumentNotNull(item, "item");

      this.CreateTemplate(item);
      this.CreateItem(item);
      this.CreateItemFields(item);
      this.SetFullPath(item);
      this.AddChildren(item);
      this.SetAccess(item);
    }

    public virtual void Add(DbTemplate template)
    {
      Assert.ArgumentNotNull(template, "template");

      if (ID.IsNullOrEmpty(template.ID))
      {
        template.ID = ID.NewID;
      }
      else
      {
        Assert.ArgumentCondition(!this.DataStorage.FakeTemplates.ContainsKey(template.ID), "template", "A tamplete with the same id has already been added.");
      }

      this.DataStorage.FakeTemplates.Add(template.ID, template);
    }

    protected virtual void CreateTemplate(DbItem item)
    {
      var fields = new DbFieldCollection();
      foreach (var itemField in item.Fields)
      {
        var templatefield = new DbField(itemField.Name) { ID = itemField.ID };
        fields.Add(templatefield);
      }

      var template = new DbTemplate(item.Name, item.TemplateID) { Fields = fields };

      this.Add(template);
    }

    protected virtual void CreateItem(DbItem item)
    {
      if (ID.IsNullOrEmpty(item.ParentID))
      {
        item.ParentID = DefaultItemRoot;
      }

      var root = this.Database.GetItem(item.ParentID);
      ItemManager.CreateItem(item.Name, root, item.TemplateID, item.ID);
    }

    protected virtual void CreateItemFields(DbItem item)
    {
      if (!item.Fields.Any())
      {
        return;
      }

      var dbitem = this.DataStorage.FakeItems[item.ID];
      foreach (var field in item.Fields)
      {
        dbitem.Fields.Add(field);
      }
    }

    protected virtual void SetFullPath(DbItem item)
    {
      if (item.ParentID == DefaultItemRoot)
      {
        item.FullPath = Constants.ContentPath + "/" + item.Name;
        return;
      }

      var parent = this.DataStorage.GetFakeItem(item.ParentID);
      item.FullPath = parent.FullPath + "/" + item.Name;
    }

    protected virtual void AddChildren(DbItem item)
    {
      foreach (var child in item.Children)
      {
        child.ParentID = item.ID;
        child.FullPath = item.FullPath + "/" + child.Name;
        this.Add(child);
      }
    }

    private void SetAccess(DbItem item)
    {
      Assert.ArgumentNotNull(item, "item");

      var fakeItem = this.DataStorage.GetFakeItem(item.ID);

      fakeItem.Access = item.Access;
    }

    public Item GetItem(string path)
    {
      Assert.ArgumentNotNullOrEmpty(path, "path");

      return this.Database.GetItem(path);
    }

    public Item GetItem(string path, string language)
    {
      Assert.ArgumentNotNullOrEmpty(language, "language");

      return this.Database.GetItem(path, Language.Parse(language));
    }

    public Item GetItem(ID id)
    {
      return this.Database.GetItem(id);
    }

    public Item GetItem(ID id, string language)
    {
      Assert.ArgumentNotNullOrEmpty(language, "language");

      return this.Database.GetItem(id, Language.Parse(language));
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
      Factory.Reset();
    }
  }
}