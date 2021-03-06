using System;
using System.Collections.Generic;
using System.Text;
using Plugins;
using System.Data;
using DevExpress.XtraEditors;
using CDTLib;

namespace TaoSoLSX
{
    public class TaoSoLSX : ICData
    {
        DataCustomData _data;
        InfoCustomData _info = new InfoCustomData(IDataType.MasterDetailDt);
        #region ICData Members

        public DataCustomData Data
        {
            set { _data = value; }
        }

        public void ExecuteAfter()
        {
            DataView dv = new DataView(_data.DsData.Tables[1]);
            dv.RowStateFilter = DataViewRowState.Added | DataViewRowState.Deleted | DataViewRowState.ModifiedCurrent;
            string sql = "update DTDonHang set SLSX = isnull((select sum(SLSX) from DTLSX where DTDHID = '{0}'),0) where DTDHID = '{0}'";
            foreach (DataRowView drv in dv)
            {
                string DTDHID = drv["DTDHID"].ToString();
                if (!_data.DbData.UpdateByNonQuery(string.Format(sql, DTDHID)))
                    break;
            }
        }

        public void ExecuteBefore()
        {
        }

        public InfoCustomData Info
        {
            get { return _info; }
        }

        #endregion
    }
}
