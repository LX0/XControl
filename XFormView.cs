using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

//using System.Windows.Forms;

using System.Drawing;
using System.Diagnostics;

using System.ComponentModel.Design;
using System.Globalization;
using System.Web.UI.Design;
using System.Reflection;
using System.Web.UI.Design.WebControls;
using XCode.Configuration;
using XCode.Attributes;

namespace XControl
{
    /// <summary>
    /// ��дFormView
    /// </summary>
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:XFormView runat=server></{0}:XFormView>")]
    [Designer(typeof(XFormViewDesigner))]
    [ToolboxBitmap(typeof(FormView))]
    public class XFormView : FormView
    {
        #region �Զ�ˢ�¶�Ӧ��XGridView
        /// <summary>
        /// �Զ�ˢ�¶�Ӧ��XGridView
        /// </summary>
        [Category(" ר������"), DefaultValue(false), Description("�Զ�ˢ�¶�Ӧ��XGridView")]
        public Boolean AutoRefreshXGridView
        {
            get
            {
                return ViewState["AutoRefreshXGridView"] == null ? false : (Boolean)ViewState["AutoRefreshXGridView"];
            }
            set
            {
                ViewState["AutoRefreshXGridView"] = value;
            }
        }

        /// <summary>
        /// �����ء�
        /// </summary>
        /// <param name="e"></param>
        protected override void OnItemInserted(FormViewInsertedEventArgs e)
        {
            base.OnItemInserted(e);
            RefreshXGridView();
        }

        /// <summary>
        /// �����ء�
        /// </summary>
        /// <param name="e"></param>
        protected override void OnItemUpdated(FormViewUpdatedEventArgs e)
        {
            base.OnItemUpdated(e);
            RefreshXGridView();
        }

        /// <summary>
        /// �����ء�
        /// </summary>
        /// <param name="e"></param>
        protected override void OnItemDeleted(FormViewDeletedEventArgs e)
        {
            base.OnItemDeleted(e);
            RefreshXGridView();
        }

        /// <summary>
        /// ���ù�����XGridView���°�����
        /// </summary>
        private void RefreshXGridView()
        {
            if (!AutoRefreshXGridView) return;
            //�ҵ���Ӧ��ObjectDataSource
            if (String.IsNullOrEmpty(DataSourceID)) return;
            ObjectDataSource ods = Find(Page, DataSourceID) as ObjectDataSource;
            if (ods == null) return;
            if (ods.SelectParameters.Count != 1) return;
            ControlParameter para = ods.SelectParameters[0] as ControlParameter;
            if (para == null || String.IsNullOrEmpty(para.ControlID)) return;
            XGridView xgv = Find(Page, para.ControlID) as XGridView;
            if (xgv == null) return;
            xgv.DataBind();
        }

        private Control Find(Control control, String id)
        {
            if (control == null || String.IsNullOrEmpty(id)) return null;
            if (control.ID == id) return control;
            if (control.Controls == null || control.Controls.Count < 1) return null;
            foreach (Control w in control.Controls)
                if (w.ID == id) return w;
            foreach (Control w in control.Controls)
            {
                Control webc = Find(w, id);
                if (webc != null) return webc;
            }
            return null;
        }
        #endregion
    }

    /// <summary>
    /// �ڿ��ӻ��������Ϊ XControl.XFormView �ؼ��ṩ���ʱ֧�֡�
    /// </summary>
    public class XFormViewDesigner : FormViewDesigner
    {
        /// <summary>
        /// �������ؼ�������Դ�ܹ�����ʱ������������
        /// </summary>
        protected override void OnSchemaRefreshed()
        {
            base.OnSchemaRefreshed();
            //������ԭ���ģ��������µ�
            if (!InTemplateMode)
            {
                try
                {
                    AddTemplates();
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                    MsgBox(ex.Message);
                }
            }
        }

        private ISite Site
        {
            get
            {
                return base.Component.Site;
            }
        }

        private void AddTemplates()
        {
            //ȡ��ʵ���࣬����Ƿ���XCode��ʵ����
            Type t = GetEntryType();
            if (t == null) return;
            List<FieldItem> list = Config.AllFields(t);
            if (list == null) return;

            //˼·
            //����ʵ�����Ա����������ģ��

            StringBuilder sbEdit = new StringBuilder();
            StringBuilder sbItem = new StringBuilder();
            StringBuilder sbInsert = new StringBuilder();
            IDesignerHost service = (IDesignerHost)Site.GetService(typeof(IDesignerHost));
            if (service == null) return;

            #region ѭ������ģ��
            String table = "<table border=\"1\" style=\"border-color:#82A8CF;border-width:1px;border-style:Solid;text-decoration:none;border-collapse:collapse;\">";
            sbEdit.Append(table);
            sbItem.Append(table);
            sbInsert.Append(table);

            String tdStyle = " style=\"border-color:#82A8CF;border-width:1px;border-style:Solid;\"";
            String tdStr = "<td" + tdStyle + ">";
            foreach (FieldItem fi in list)
            {
                //��Biz���������������ԣ���Ӧ�ó�����XFormView��
                if (String.IsNullOrEmpty(fi.ColumnName)) continue;

                sbEdit.Append("<tr>");
                sbItem.Append("<tr>");
                if (!fi.DataObjectField.IsIdentity) sbInsert.Append("<tr>");

                String name = fi.Name;
                //����õ�һ�����֣�ֻ������ĸ���ֺ��»��ߣ������ַ�תΪ�»���
                char[] chArray = new char[name.Length];
                for (int i = 0; i < name.Length; i++)
                {
                    char c = name[i];
                    if (char.IsLetterOrDigit(c) || (c == '_'))
                    {
                        chArray[i] = c;
                    }
                    else
                    {
                        chArray[i] = '_';
                    }
                }
                String controlID = new String(chArray);
                String str3 = "Eval(\"" + name + "\")";
                String str4 = "Bind(\"" + name + "\")";
                if (fi.Info.PropertyType == typeof(DateTime))
                {
                    str3 = "Eval(\"" + name + "\", \"{0:yyyy-MM-dd HH:mm:ss}\")";
                    str4 = "Bind(\"" + name + "\", \"{0:yyyy-MM-dd HH:mm:ss}\")";
                }
                //����ָ��Ϊ������
                name = (String.IsNullOrEmpty(fi.CnName)) ? fi.Name : fi.CnName;
                //ǰ׺
                String perfix = "XCL";
                if (fi.DataObjectField.PrimaryKey || fi.DataObjectField.IsIdentity)
                {
                    sbEdit.Append(String.Format(CultureInfo.InvariantCulture, "{3}{0}��</td>{3} <asp:Label Text='<%# {1} %>' runat=\"server\" id=\"{2}Label1\" /></td>", new object[] { name, str3, controlID, tdStr }));
                    sbItem.Append(String.Format(CultureInfo.InvariantCulture, "{3}{0}��</td>{3} <asp:Label Text='<%# {1} %>' runat=\"server\" id=\"{2}Label\" /></td>", new object[] { name, str3, controlID, tdStr }));
                    if (!fi.DataObjectField.IsIdentity)
                    {
                        sbInsert.Append(String.Format(CultureInfo.InvariantCulture, "{3}{0}��</td>{3} <asp:TextBox Text='<%# {1} %>' runat=\"server\" id=\"{2}TextBox\" /></td>", new object[] { name, str4, controlID, tdStr }));
                    }
                }
                //�����ͣ�������Is��ͷ�ҵ�����ĸ�Ǵ�д��ĸ�����ͣ�����IsTop
                else if (fi.Info.PropertyType == typeof(Boolean))
                {
                    sbEdit.Append(String.Format(CultureInfo.InvariantCulture, "{3}{0}��</td>{3} <asp:CheckBox Checked='<%# {1} %>' runat=\"server\" id=\"{2}CheckBox\" /></td>", new object[] { name, str4, controlID, tdStr }));
                    sbItem.Append(String.Format(CultureInfo.InvariantCulture, "{3}{0}��</td>{3} <asp:CheckBox Checked='<%# {1} %>' runat=\"server\" id=\"{2}CheckBox\" Enabled=\"false\" /></td>", new object[] { name, str4, controlID, tdStr }));
                    sbInsert.Append(String.Format(CultureInfo.InvariantCulture, "{3}{0}��</td>{3} <asp:CheckBox Checked='<%# {1} %>' runat=\"server\" id=\"{2}CheckBox\" /></td>", new object[] { name, str4, controlID, tdStr }));
                }
                else if (fi.Info.PropertyType == typeof(Int32))
                {
                    if (fi.Info.PropertyType == typeof(Int32) && fi.Name.Length > 2 &&
                    fi.Name.StartsWith("Is") && fi.Name[2] >= 'A' && fi.Name[2] <= 'Z')
                    {
                        sbEdit.Append(String.Format(CultureInfo.InvariantCulture, "{3}{0}��</td>{3} <{4}:IntCheckBox Value='<%# {1} %>' runat=\"server\" id=\"{2}IntCheckBox\" /></td>", new object[] { name, str4, controlID, tdStr, perfix }));
                        sbItem.Append(String.Format(CultureInfo.InvariantCulture, "{3}{0}��</td>{3} <{4}:IntCheckBox Value='<%# {1} %>' runat=\"server\" id=\"{2}IntCheckBox\" Enabled=\"false\" /></td>", new object[] { name, str4, controlID, tdStr, perfix }));
                        sbInsert.Append(String.Format(CultureInfo.InvariantCulture, "{3}{0}��</td>{3} <{4}:IntCheckBox Value='<%# {1} %>' runat=\"server\" id=\"{2}IntCheckBox\" /></td>", new object[] { name, str4, controlID, tdStr, perfix }));
                    }
                    else
                    {
                        sbEdit.Append(String.Format(CultureInfo.InvariantCulture, "{3}{0}��</td>{3} <{4}:NumberBox Text='<%# {1} %>' runat=\"server\" id=\"{2}NumberBox\" /></td>", new object[] { name, str4, controlID, tdStr, perfix }));
                        sbItem.Append(String.Format(CultureInfo.InvariantCulture, "{3}{0}��</td>{3} <asp:Label Text='<%# {1} %>' runat=\"server\" id=\"{2}Label\" /></td>", new object[] { name, str4, controlID, tdStr, perfix }));
                        sbInsert.Append(String.Format(CultureInfo.InvariantCulture, "{3}{0}��</td>{3} <{4}:NumberBox Text='<%# {1} %>' runat=\"server\" id=\"{2}NumberBox\" /></td>", new object[] { name, str4, controlID, tdStr, perfix }));
                    }
                }
                else if (fi.Info.PropertyType == typeof(Double))
                {
                    sbEdit.Append(String.Format(CultureInfo.InvariantCulture, "{3}{0}��</td>{3} <{4}:RealBox Text='<%# {1} %>' runat=\"server\" id=\"{2}RealBox\" /></td>", new object[] { name, str4, controlID, tdStr, perfix }));
                    sbItem.Append(String.Format(CultureInfo.InvariantCulture, "{3}{0}��</td>{3} <asp:Label Text='<%# {1} %>' runat=\"server\" id=\"{2}Label\" /></td>", new object[] { name, str4, controlID, tdStr, perfix }));
                    sbInsert.Append(String.Format(CultureInfo.InvariantCulture, "{3}{0}��</td>{3} <{4}:RealBox Text='<%# {1} %>' runat=\"server\" id=\"{2}RealBox\" /></td>", new object[] { name, str4, controlID, tdStr, perfix }));
                }
                else if (fi.Info.PropertyType == typeof(DateTime))
                {
                    sbEdit.Append(String.Format(CultureInfo.InvariantCulture, "{3}{0}��</td>{3} <{4}:DateBox Text='<%# {1} %>' runat=\"server\" id=\"{2}DateBox\" /></td>", new object[] { name, str4, controlID, tdStr, perfix }));
                    sbItem.Append(String.Format(CultureInfo.InvariantCulture, "{3}{0}��</td>{3} <asp:Label Text='<%# {1} %>' runat=\"server\" id=\"{2}Label\" /></td>", new object[] { name, str4, controlID, tdStr, perfix }));
                    sbInsert.Append(String.Format(CultureInfo.InvariantCulture, "{3}{0}��</td>{3} <{4}:DateBox Text='<%# {1} %>' runat=\"server\" id=\"{2}DateBox\" /></td>", new object[] { name, str4, controlID, tdStr, perfix }));
                }
                else if (fi.Name.ToLower() == "ip")
                {
                    sbEdit.Append(String.Format(CultureInfo.InvariantCulture, "{3}{0}��</td>{3} <{4}:IPBox Text='<%# {1} %>' runat=\"server\" id=\"{2}IPBox\" /></td>", new object[] { name, str4, controlID, tdStr, perfix }));
                    sbItem.Append(String.Format(CultureInfo.InvariantCulture, "{3}{0}��</td>{3} <asp:Label Text='<%# {1} %>' runat=\"server\" id=\"{2}Label\" /></td>", new object[] { name, str4, controlID, tdStr, perfix }));
                    sbInsert.Append(String.Format(CultureInfo.InvariantCulture, "{3}{0}��</td>{3} <{4}:IPBox Text='<%# {1} %>' runat=\"server\" id=\"{2}IPBox\" /></td>", new object[] { name, str4, controlID, tdStr, perfix }));
                }
                else if (fi.Name.ToLower() == "mail" || fi.Name.ToLower() == "email")
                {
                    sbEdit.Append(String.Format(CultureInfo.InvariantCulture, "{3}{0}��</td>{3} <{4}:MailBox Text='<%# {1} %>' runat=\"server\" id=\"{2}MailBox\" /></td>", new object[] { name, str4, controlID, tdStr, perfix }));
                    sbItem.Append(String.Format(CultureInfo.InvariantCulture, "{3}{0}��</td>{3} <asp:Label Text='<%# {1} %>' runat=\"server\" id=\"{2}Label\" /></td>", new object[] { name, str4, controlID, tdStr, perfix }));
                    sbInsert.Append(String.Format(CultureInfo.InvariantCulture, "{3}{0}��</td>{3} <{4}:MailBox Text='<%# {1} %>' runat=\"server\" id=\"{2}MailBox\" /></td>", new object[] { name, str4, controlID, tdStr, perfix }));
                }
                else if (fi.Name.ToLower() == "password")
                {
                    sbEdit.Append(String.Format(CultureInfo.InvariantCulture, "{3}{0}��</td>{3} <asp:TextBox Text='<%# {1} %>' runat=\"server\" id=\"{2}TextBox\"  TextMode=\"Password\" /></td>", new object[] { name, str4, controlID, tdStr }));
                    sbItem.Append(String.Format(CultureInfo.InvariantCulture, "{3}{0}��</td>{3} <asp:Label Text='<%# {1} %>' runat=\"server\" id=\"{2}Label\" /></td>", new object[] { name, str4, controlID, tdStr }));
                    sbInsert.Append(String.Format(CultureInfo.InvariantCulture, "{3}{0}��</td>{3} <asp:TextBox Text='<%# {1} %>' runat=\"server\" id=\"{2}TextBox\" TextMode=\"Password\" /></td>", new object[] { name, str4, controlID, tdStr }));
                }
                else if (fi.DataObjectField.Length > 255)
                {
                    sbEdit.Append(String.Format(CultureInfo.InvariantCulture, "{3}{0}��</td>{3} <asp:TextBox Text='<%# {1} %>' runat=\"server\" id=\"{2}TextBox\" Height=\"130px\" TextMode=\"MultiLine\" Width=\"440px\" /></td>", new object[] { name, str4, controlID, tdStr }));
                    sbItem.Append(String.Format(CultureInfo.InvariantCulture, "{3}{0}��</td>{3} <asp:Label Text='<%# {1} %>' runat=\"server\" id=\"{2}Label\" /></td>", new object[] { name, str4, controlID, tdStr }));
                    sbInsert.Append(String.Format(CultureInfo.InvariantCulture, "{3}{0}��</td>{3} <asp:TextBox Text='<%# {1} %>' runat=\"server\" id=\"{2}TextBox\" Height=\"130px\" TextMode=\"MultiLine\" Width=\"440px\" /></td>", new object[] { name, str4, controlID, tdStr }));
                }
                else
                {
                    sbEdit.Append(String.Format(CultureInfo.InvariantCulture, "{3}{0}��</td>{3} <asp:TextBox Text='<%# {1} %>' runat=\"server\" id=\"{2}TextBox\" /></td>", new object[] { name, str4, controlID, tdStr }));
                    sbItem.Append(String.Format(CultureInfo.InvariantCulture, "{3}{0}��</td>{3} <asp:Label Text='<%# {1} %>' runat=\"server\" id=\"{2}Label\" /></td>", new object[] { name, str4, controlID, tdStr }));
                    sbInsert.Append(String.Format(CultureInfo.InvariantCulture, "{3}{0}��</td>{3} <asp:TextBox Text='<%# {1} %>' runat=\"server\" id=\"{2}TextBox\" /></td>", new object[] { name, str4, controlID, tdStr }));
                }
                sbEdit.Append(Environment.NewLine);
                sbItem.Append(Environment.NewLine);
                sbInsert.Append(Environment.NewLine);

                sbEdit.Append("</tr>");
                sbItem.Append("</tr>");
                if (!fi.DataObjectField.IsIdentity) sbInsert.Append("</tr>");
            }

            sbEdit.Append("<tr><td colspan=2" + tdStyle + ">");
            sbItem.Append("<tr><td colspan=2" + tdStyle + ">");
            sbInsert.Append("<tr><td colspan=2" + tdStyle + ">");

            bool flag = true;
            if (base.DesignerView.CanUpdate)
            {
                sbItem.Append(String.Format(CultureInfo.InvariantCulture, "<asp:LinkButton runat=\"server\" Text=\"{3}\" CommandName=\"{0}\" id=\"{1}{0}Button\" CausesValidation=\"{2}\" />", new object[] { "Edit", String.Empty, bool.FalseString, "�༭" }));
                flag = false;
            }
            sbEdit.Append(String.Format(CultureInfo.InvariantCulture, "<asp:LinkButton runat=\"server\" Text=\"{3}\" CommandName=\"{0}\" id=\"{1}{0}Button\" CausesValidation=\"{2}\" />", new object[] { "Update", String.Empty, bool.TrueString, "����" }));
            sbEdit.Append("&nbsp;");
            sbEdit.Append(String.Format(CultureInfo.InvariantCulture, "<asp:LinkButton runat=\"server\" Text=\"{3}\" CommandName=\"{0}\" id=\"{1}{0}Button\" CausesValidation=\"{2}\" />", new object[] { "Cancel", "Update", bool.FalseString, "ȡ��" }));
            if (base.DesignerView.CanDelete)
            {
                if (!flag)
                {
                    sbItem.Append("&nbsp;");
                }
                sbItem.Append(String.Format(CultureInfo.InvariantCulture, "<asp:LinkButton runat=\"server\" Text=\"{3}\" CommandName=\"{0}\" id=\"{1}{0}Button\" CausesValidation=\"{2}\" />", new object[] { "Delete", String.Empty, bool.FalseString, "ɾ��" }));
                flag = false;
            }
            if (base.DesignerView.CanInsert)
            {
                if (!flag)
                {
                    sbItem.Append("&nbsp;");
                }
                sbItem.Append(String.Format(CultureInfo.InvariantCulture, "<asp:LinkButton runat=\"server\" Text=\"{3}\" CommandName=\"{0}\" id=\"{1}{0}Button\" CausesValidation=\"{2}\" />", new object[] { "New", String.Empty, bool.FalseString, "�½�" }));
            }
            sbInsert.Append(String.Format(CultureInfo.InvariantCulture, "<asp:LinkButton runat=\"server\" Text=\"{3}\" CommandName=\"{0}\" id=\"{1}{0}Button\" CausesValidation=\"{2}\" />", new object[] { "Insert", String.Empty, bool.TrueString, "����" }));
            sbInsert.Append("&nbsp;");
            sbInsert.Append(String.Format(CultureInfo.InvariantCulture, "<asp:LinkButton runat=\"server\" Text=\"{3}\" CommandName=\"{0}\" id=\"{1}{0}Button\" CausesValidation=\"{2}\" />", new object[] { "Cancel", "Insert", bool.FalseString, "ȡ��" }));

            sbEdit.Append("</td></tr>");
            sbItem.Append("</td></tr>");
            sbInsert.Append("</td></tr>");

            sbEdit.Append(Environment.NewLine);
            sbItem.Append(Environment.NewLine);
            sbInsert.Append(Environment.NewLine);

            sbEdit.Append("</table>");
            sbItem.Append("</table>");
            sbInsert.Append("</table>");
            #endregion

            try
            {
                XFormView fv = base.Component as XFormView;
                if (fv != null)
                {
                    fv.EditItemTemplate = ControlParser.ParseTemplate(service, sbEdit.ToString());
                    fv.ItemTemplate = ControlParser.ParseTemplate(service, sbItem.ToString());
                    fv.InsertItemTemplate = ControlParser.ParseTemplate(service, sbInsert.ToString());
                    BindTableAttribute[] btas = t.GetCustomAttributes(typeof(BindTableAttribute), false) as BindTableAttribute[];
                    if (btas == null || btas.Length < 1 || String.IsNullOrEmpty(btas[0].Description))
                        fv.EmptyDataTemplate = ControlParser.ParseTemplate(service, "<asp:LinkButton ID=\"LinkButton1\" runat=\"server\" CommandName=\"New\">����</asp:LinkButton>");
                    else
                        fv.EmptyDataTemplate = ControlParser.ParseTemplate(service, "<asp:LinkButton ID=\"LinkButton1\" runat=\"server\" CommandName=\"New\">����" + btas[0].Description + "</asp:LinkButton>");

                    //���⹤��������AutoRefreshXGridView
                    fv.AutoRefreshXGridView = true;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// ȡ��ʵ������
        /// </summary>
        /// <returns></returns>
        private Type GetEntryType()
        {
            if (this.Site == null || this.Site.Component == null || !(Site.Component is XFormView)) return null;
            String datasourceid = (Site.Component as XFormView).DataSourceID;
            if (String.IsNullOrEmpty(datasourceid)) return null;
            // �ҵ����ݰ󶨿ؼ�ObjectDataSource
            XFormView fv = base.Component as XFormView;
            if (fv == null || fv.Page == null) return null;
            ObjectDataSource obj = fv.Page.FindControl(datasourceid) as ObjectDataSource;
            if (obj == null) return null;
            // �ҵ�ʵ������
            String typeName = obj.DataObjectTypeName;
            if (String.IsNullOrEmpty(typeName)) typeName = obj.TypeName;
            Type t = Type.GetType(typeName);
            if (t == null)
            {
                t = System.Web.Compilation.BuildManager.GetType(typeName, false, true);
                if (t == null)
                {
                    Assembly[] abs = AppDomain.CurrentDomain.GetAssemblies();
                    foreach (Assembly ab in abs)
                    {
                        t = ab.GetType(typeName, false, true);
                        if (t != null) break;
                    }
                    if (t == null)
                    {
                        MsgBox("�޷���λ��������ࣺ" + typeName + "����������Ҫ����һ�����������������Ŀ��");
                        return null;
                    }
                }
            }
            return t;
            //// ����ʵ�����Ƿ�̳���XCode.Entry.Entry
            //while (t.BaseType != typeof(Object))
            //{
            //    //if (t.BaseType == typeof(XCode.Entry.Entry)) return t;
            //    if (t.BaseType.Name == "Entry") return t;
            //    t = t.BaseType;
            //}
            //return null;
        }

        private static void MsgBox(String msg)
        {
            XLog.Trace.Debug();
            System.Windows.Forms.MessageBox.Show(msg + "\n��־Ŀ¼��" + XLog.Trace.LogDir, "XFormView");
        }
    }
}