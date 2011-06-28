using System;
using System.Collections.Generic;
using System.Text;

using System.Web.UI.WebControls;
using System.Drawing;
using System.ComponentModel;
using System.Web.UI;

namespace XControl
{
    /// <summary>
    /// ����������ؼ���ֻ���������֣������Թ涨��Χ�������
    /// </summary>
    [Description("����������ؼ�")]
    [ToolboxData("<{0}:RealBox runat=server></{0}:RealBox>")]
    [ToolboxBitmap(typeof(TextBox))]
    [ControlValueProperty("Value")]
    public class RealBox : TextBox
    {
        /// <summary>
        /// ��ʼ����������ؼ�����ʽ��
        /// </summary>
        public RealBox()
            : base()
        {
            this.ToolTip = "ֻ�����븡������";
            BorderWidth = Unit.Pixel(0);
            BorderColor = Color.Black;
            BorderStyle = BorderStyle.Solid;
            Font.Size = FontUnit.Point(10);
            Width = Unit.Pixel(70);
            if (String.IsNullOrEmpty(Attributes["style"])) this.Attributes.Add("style", "border-bottom-width:1px;text-align : right ");
        }

        /// <summary>
        /// �����ء�
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // У��ű�
            this.Attributes.Add("onkeypress", "return ValidReal();");
            this.Attributes.Add("onblur", "return ValidReal2();");
            this.Page.ClientScript.RegisterClientScriptResource(typeof(NumberBox), "XControl.TextBox.Validator.js");
        }

        /// <summary>
        /// ��ǰֵ
        /// </summary>
        [Category(" ר������"), DefaultValue(0), Description("��ǰֵ")]
        public Double Value
        {
            get
            {
                if (String.IsNullOrEmpty(Text)) return Double.NaN;
                Double k = 0;
                if (!Double.TryParse(Text, out k)) return Double.NaN;
                return k;
            }
            set
            {
                Text = value.ToString();
            }
        }
    }
}
