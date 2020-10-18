// <auto-generated />
#pragma warning disable CS0105
using MasterMemory.Validation;
using MasterMemory;
using MessagePack;
using Sakaba.Domain;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using MD.Tables;

namespace MD
{
   public sealed class MemoryDatabase : MemoryDatabaseBase
   {
        public ItemTable ItemTable { get; private set; }
        public ItemTierTable ItemTierTable { get; private set; }

        public MemoryDatabase(
            ItemTable ItemTable,
            ItemTierTable ItemTierTable
        )
        {
            this.ItemTable = ItemTable;
            this.ItemTierTable = ItemTierTable;
        }

        public MemoryDatabase(byte[] databaseBinary, bool internString = true, MessagePack.IFormatterResolver formatterResolver = null)
            : base(databaseBinary, internString, formatterResolver)
        {
        }

        protected override void Init(Dictionary<string, (int offset, int count)> header, System.ReadOnlyMemory<byte> databaseBinary, MessagePack.MessagePackSerializerOptions options)
        {
            this.ItemTable = ExtractTableData<Item, ItemTable>(header, databaseBinary, options, xs => new ItemTable(xs));
            this.ItemTierTable = ExtractTableData<ItemTier, ItemTierTable>(header, databaseBinary, options, xs => new ItemTierTable(xs));
        }

        public ImmutableBuilder ToImmutableBuilder()
        {
            return new ImmutableBuilder(this);
        }

        public DatabaseBuilder ToDatabaseBuilder()
        {
            var builder = new DatabaseBuilder();
            builder.Append(this.ItemTable.GetRawDataUnsafe());
            builder.Append(this.ItemTierTable.GetRawDataUnsafe());
            return builder;
        }

        public ValidateResult Validate()
        {
            var result = new ValidateResult();
            var database = new ValidationDatabase(new object[]
            {
                ItemTable,
                ItemTierTable,
            });

            ((ITableUniqueValidate)ItemTable).ValidateUnique(result);
            ValidateTable(ItemTable.All, database, "Id", ItemTable.PrimaryKeySelector, result);
            ((ITableUniqueValidate)ItemTierTable).ValidateUnique(result);
            ValidateTable(ItemTierTable.All, database, "Id", ItemTierTable.PrimaryKeySelector, result);

            return result;
        }

        static MasterMemory.Meta.MetaDatabase metaTable;

        public static object GetTable(MemoryDatabase db, string tableName)
        {
            switch (tableName)
            {
                case "Item":
                    return db.ItemTable;
                case "ItemTier":
                    return db.ItemTierTable;
                
                default:
                    return null;
            }
        }

        public static MasterMemory.Meta.MetaDatabase GetMetaDatabase()
        {
            if (metaTable != null) return metaTable;

            var dict = new Dictionary<string, MasterMemory.Meta.MetaTable>();
            dict.Add("Item", MD.Tables.ItemTable.CreateMetaTable());
            dict.Add("ItemTier", MD.Tables.ItemTierTable.CreateMetaTable());

            metaTable = new MasterMemory.Meta.MetaDatabase(dict);
            return metaTable;
        }
    }
}