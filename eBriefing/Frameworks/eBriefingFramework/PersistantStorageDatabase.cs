/*
Copyright (C) 2017 MetroStar Systems

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

The full license text can be found is the included LICENSE file.

You can freely use any of this software which you make publicly 
available at no charge.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>
*/

using System;
using Metrostar.Mobile.Framework;
using System.Collections.Generic;

namespace eBriefingMobile
{
    /// <summary>
    /// Used to store an set of tables into a database.
    /// </summary>
    public class PersistantStorageDatabase<T>
    {
        protected PersistantStorageDatabase()
        {
        }

        public PersistantStorageDatabase(String databaseName)
        {
            _databaseName = databaseName;
        }

        public String DatabaseName
        {
            get { return _databaseName; }
            
        }

        private String _databaseName = String.Empty;
        private Dictionary<String, Dictionary<String, T>> _cache = new Dictionary<String, Dictionary<String, T>>();

        private Dictionary<String, T> GetTable(String tableId)
        {
            Dictionary<String, T> cachedTable = GetCachedTable(tableId);
            if (cachedTable != null)
            {
                return cachedTable;
            }
            else
            {
                Dictionary<String, T> table = LoadTable(tableId);
                SetCachedTable(tableId, table);
                return table;
            }
        }

        private Dictionary<String, T> GetCachedTable(String tableId)
        {
            lock (_cache)
            {
                if (_cache.ContainsKey(tableId) && (_cache[tableId] != null))
                {
                    return _cache[tableId];
                }
                else
                {
                    return null;
                }
            }
        }

        private void SetCachedTable(String tableId, Dictionary<String, T> table)
        {
            lock (_cache)
            {
                _cache[tableId] = table;
            }
        }

        private void DeleteCachedTable(String tableId)
        {
            lock (_cache)
            {
                if (_cache.ContainsKey(tableId))
                {
                    _cache.Remove(tableId);
                }
            }
        }

        private Dictionary<String, T> LoadTable(String tableId)
        {
            // NOTE: This method abstracts away just how and where we store the table.

            Dictionary<String, T> table = null;

            String key = BuildTableKey(tableId);
            object valueRaw = PersistantObjectsStorage.GetValue(key);
            if (valueRaw == null)
            {
                table = new Dictionary<String, T>();
            }
            else
            {
                table = (Dictionary<String,T>)valueRaw;
            }

            return table;
        }

        private void SaveTable(String tableId)
        {
            // NOTE: This method abstracts away just how and where we store the table.
            Dictionary<String, T> cachedTable = GetCachedTable(tableId);
            if (cachedTable != null)
            {
                String key = BuildTableKey(tableId);
                PersistantObjectsStorage.Add(key, cachedTable);
            }
        }

        private String BuildTableKey(String tableId)
        {
            return String.Format("{0}_Table_{1}", _databaseName, tableId);
        }

        internal T GetRecord(String tableId, String recordId)
        {
            Dictionary<String, T> table = GetTable(tableId);
            if (table.ContainsKey(recordId))
            {
                return table[recordId];
            }

            return default(T);
        }

        internal List<T> GetRecordsInTable(String tableId)
        {
            Dictionary<String, T> table = GetTable(tableId);
            if (table != null)
            {
                List<T> result = null;
                if (table.Values != null)
                {
                    foreach (T value in table.Values)
                    {
                        if (result == null)
                        {
                            result = new List<T>();
                        }

                        result.Add(value);
                    }
                }

                return result;
            }

            return null;
        }

        internal bool HasRecord(String tableId, String recordId)
        {
            Dictionary<String, T> table = GetTable(tableId);
            if (table.ContainsKey(recordId))
            {
                var value = table[recordId];
                if (value != null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

            return false;
        }

		public void DeleteRecord(String tableId, String recordId)
        {
            Dictionary<String, T> table = GetTable(tableId);
            if (table.ContainsKey(recordId))
            {
                table.Remove(recordId);
                SaveTable(tableId);
            }
        }

        internal void DeleteTable(String tableId)
        {
            Dictionary<String, T> table = GetTable(tableId);
            if (table != null)
            {
                table.Clear();
                SaveTable(tableId);
                DeleteCachedTable(tableId);
            }
        }

        internal void SetRecord(String tableId, String recordId, T record)
        {
            Dictionary<String, T> table = GetTable(tableId);
            if (record == null)
            {
                if (table.ContainsKey(recordId))
                {
                    table.Remove(recordId);
                    SaveTable(tableId);
                }
            }
            else
            {
                table[recordId] = record;
                SaveTable(tableId);
            }
        }

        internal void SetRecordBatch(String tableId, String recordId, T record)
        {
            Dictionary<String, T> table = GetTable(tableId);
            table[recordId] = record;
            
            // Do not save here since we are changing a lot of them in batch
        }

        internal void ClearTable(String tableId)
        {
            // Prime the table from storage if it is not already loaded.
            Dictionary<String, T> table = GetTable(tableId);

            table.Clear();
        }

        internal void StartBatchUpdate(String tableId)
        {
            // Prime the table from storage if it is not already loaded.
            GetTable(tableId);
        }

        internal void EndBatchUpdate(String tableId)
        {
            SaveTable(tableId);
        }

        internal int GetNumberOfRecordsInTable(String tableId)
        {
            Dictionary<String, T> table = GetTable(tableId);
            if (table != null)
            {
                return table.Count;
            }

            return 0;
        }
    }
}

