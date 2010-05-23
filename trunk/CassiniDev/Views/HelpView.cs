//  **********************************************************************************
//  CassiniDev - http://cassinidev.codeplex.com
// 
//  Copyright (c) Sky Sanders. All rights reserved.
//  
//  This source code is subject to terms and conditions of the Microsoft Public
//  License (Ms-PL). A copy of the license can be found in the license.htm file
//  included in this distribution.
//  
//  You must not remove this notice, or any other, from this software.
//  
//  **********************************************************************************

#region

using System.Windows.Forms;

#endregion

namespace CassiniDev
{
    public partial class HelpView : Form
    {
        public HelpView(string content)
        {
            InitializeComponent();
            base.Text = SR.GetString(SR.WebdevVwdName);
            label1.Text = content;
        }
    }
}