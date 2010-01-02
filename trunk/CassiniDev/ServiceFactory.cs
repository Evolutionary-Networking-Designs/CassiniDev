// /* **********************************************************************************
//  *
//  * Copyright (c) Sky Sanders. All rights reserved.
//  * 
//  * This source code is subject to terms and conditions of the Microsoft Public
//  * License (Ms-PL). A copy of the license can be found in the license.htm file
//  * included in this distribution.
//  *
//  * You must not remove this notice, or any other, from this software.
//  *
//  * **********************************************************************************/
using System;
using Cassini;

namespace CassiniDev
{
    /// <summary>
    /// Simple Abstract Factory and Service Locator pattern to break dependancies and 
    /// emplace a testing seam to facilitate mocking.
    /// Use [InternalsVisisbleTo..] in test project
    /// </summary>
    public static class ServiceFactory
    {

        internal static FactoryStubs Stubs { get; set; }

        private static IRules _rules;
        /// <summary>
        /// Call ResetStubs to reset factory defaults after a mocking
        /// </summary>
        internal static void ResetStubs()
        {
            Stubs = new FactoryStubs();
        }

        static ServiceFactory()
        {
            ResetStubs();
        }

        public static IView CreateConsoleView(IPresenter presenter)
        {
            return Stubs.CreateConsoleView(presenter);
        }

        public static IView CreateFormsView(IPresenter presenter)
        {
            return Stubs.CreateFormsView(presenter);
        }

        public static IPresenter CreatePresenter()
        {
            return Stubs.CreatePresenter();
        }

        public static IServer CreateServer(ServerArguments args)
        {
            return Stubs.CreateServer(args);
        }

        public static IRules Rules
        {
            get
            {
                if (_rules == null)
                {
                    _rules = Stubs.CreateRules();
                }
                return _rules;
            }
        }
        /// <summary>
        /// Implements a testing seam. use [InternalsVisisbleTo..] in test project
        /// </summary>
        internal class FactoryStubs
        {
            public Func<IPresenter, IView> CreateConsoleView = presenter => new ConsoleView(presenter);
            public Func<IPresenter, IView> CreateFormsView = presenter => new FormsView(presenter);
            public Func<IPresenter> CreatePresenter = () => new Presenter();
            public Func<ServerArguments, IServer> CreateServer = args => new Server(args);
            public Func<IRules> CreateRules = () => new Rules();
        }

    }
}