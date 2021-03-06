﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Common;
using Microsoft.UI.Private.Controls;
using MUXControlsTestApp.Utilities;
using System;
using System.Threading;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media;

#if USING_TAEF
using WEX.TestExecution;
using WEX.TestExecution.Markup;
using WEX.Logging.Interop;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
#endif

using ScrollView = Microsoft.UI.Xaml.Controls.ScrollView;
using ScrollBarVisibility = Microsoft.UI.Xaml.Controls.ScrollBarVisibility;
using ScrollPresenter = Microsoft.UI.Xaml.Controls.Primitives.ScrollPresenter;
using ContentOrientation = Microsoft.UI.Xaml.Controls.ContentOrientation;
using ScrollMode = Microsoft.UI.Xaml.Controls.ScrollMode;
using InputKind = Microsoft.UI.Xaml.Controls.InputKind;
using ChainingMode = Microsoft.UI.Xaml.Controls.ChainingMode;
using RailingMode = Microsoft.UI.Xaml.Controls.RailingMode;
using ZoomMode = Microsoft.UI.Xaml.Controls.ZoomMode;
using ScrollingAnchorRequestedEventArgs = Microsoft.UI.Xaml.Controls.ScrollingAnchorRequestedEventArgs;
using MUXControlsTestHooksLoggingMessageEventArgs = Microsoft.UI.Private.Controls.MUXControlsTestHooksLoggingMessageEventArgs;
using ScrollViewTestHooks = Microsoft.UI.Private.Controls.ScrollViewTestHooks;

namespace Windows.UI.Xaml.Tests.MUXControls.ApiTests
{
    [TestClass]
    public class ScrollViewTests : ApiTestBase
    {
        private const int c_MaxWaitDuration = 5000;
        private const double c_epsilon = 0.0000001;

        private const InputKind c_defaultIgnoredInputKind = InputKind.None;
        private const ChainingMode c_defaultHorizontalScrollChainingMode = ChainingMode.Auto;
        private const ChainingMode c_defaultVerticalScrollChainingMode = ChainingMode.Auto;
        private const RailingMode c_defaultHorizontalScrollRailingMode = RailingMode.Enabled;
        private const RailingMode c_defaultVerticalScrollRailingMode = RailingMode.Enabled;
#if USE_SCROLLMODE_AUTO
        private const ScrollMode c_defaultComputedHorizontalScrollMode = ScrollMode.Disabled;
        private const ScrollMode c_defaultComputedVerticalScrollMode = ScrollMode.Disabled;
        private const ScrollMode c_defaultHorizontalScrollMode = ScrollMode.Auto;
        private const ScrollMode c_defaultVerticalScrollMode = ScrollMode.Auto;
#else
        private const ScrollMode c_defaultHorizontalScrollMode = ScrollMode.Enabled;
        private const ScrollMode c_defaultVerticalScrollMode = ScrollMode.Enabled;
#endif
        private const ChainingMode c_defaultZoomChainingMode = ChainingMode.Auto;
        private const ZoomMode c_defaultZoomMode = ZoomMode.Disabled;
        private const ContentOrientation c_defaultContentOrientation = ContentOrientation.Vertical;
        private const double c_defaultMinZoomFactor = 0.1;
        private const double c_defaultMaxZoomFactor = 10.0;
        private const double c_defaultAnchorRatio = 0.0;

        private const double c_defaultUIScrollViewContentWidth = 1200.0;
        private const double c_defaultUIScrollViewContentHeight = 600.0;
        private const double c_defaultUIScrollViewWidth = 300.0;
        private const double c_defaultUIScrollViewHeight = 200.0;

        private ScrollViewVisualStateCounts m_scrollViewVisualStateCounts;

        [TestMethod]
        [TestProperty("Description", "Verifies the ScrollView default properties.")]
        public void VerifyDefaultPropertyValues()
        {
            if (PlatformConfiguration.IsOSVersionLessThan(OSVersion.Redstone2))
            {
                Log.Warning("Test is disabled on pre-RS2 because ScrollView not supported pre-RS2");
                return;
            }

            RunOnUIThread.Execute(() =>
            {
                ScrollView scrollView = new ScrollView();
                Verify.IsNotNull(scrollView);

                Log.Comment("Verifying ScrollView default property values");
                Verify.IsNull(scrollView.Content);
                Verify.IsNull(ScrollViewTestHooks.GetScrollPresenterPart(scrollView));
                Verify.IsNull(scrollView.HorizontalScrollController);
                Verify.IsNull(scrollView.VerticalScrollController);
#if USE_SCROLLMODE_AUTO
                Verify.AreEqual(scrollView.ComputedHorizontalScrollMode, c_defaultComputedHorizontalScrollMode);
                Verify.AreEqual(scrollView.ComputedVerticalScrollMode, c_defaultComputedVerticalScrollMode);
#endif
                Verify.AreEqual(scrollView.IgnoredInputKind, c_defaultIgnoredInputKind);
                Verify.AreEqual(scrollView.ContentOrientation, c_defaultContentOrientation);
                Verify.AreEqual(scrollView.HorizontalScrollChainingMode, c_defaultHorizontalScrollChainingMode);
                Verify.AreEqual(scrollView.VerticalScrollChainingMode, c_defaultVerticalScrollChainingMode);
                Verify.AreEqual(scrollView.HorizontalScrollRailingMode, c_defaultHorizontalScrollRailingMode);
                Verify.AreEqual(scrollView.VerticalScrollRailingMode, c_defaultVerticalScrollRailingMode);
                Verify.AreEqual(scrollView.HorizontalScrollMode, c_defaultHorizontalScrollMode);
                Verify.AreEqual(scrollView.VerticalScrollMode, c_defaultVerticalScrollMode);
                Verify.AreEqual(scrollView.ZoomMode, c_defaultZoomMode);
                Verify.AreEqual(scrollView.ZoomChainingMode, c_defaultZoomChainingMode);
                Verify.IsGreaterThan(scrollView.MinZoomFactor, c_defaultMinZoomFactor - c_epsilon);
                Verify.IsLessThan(scrollView.MinZoomFactor, c_defaultMinZoomFactor + c_epsilon);
                Verify.IsGreaterThan(scrollView.MaxZoomFactor, c_defaultMaxZoomFactor - c_epsilon);
                Verify.IsLessThan(scrollView.MaxZoomFactor, c_defaultMaxZoomFactor + c_epsilon);
                Verify.AreEqual(scrollView.HorizontalAnchorRatio, c_defaultAnchorRatio);
                Verify.AreEqual(scrollView.VerticalAnchorRatio, c_defaultAnchorRatio);
                Verify.AreEqual(scrollView.ExtentWidth, 0.0);
                Verify.AreEqual(scrollView.ExtentHeight, 0.0);
                Verify.AreEqual(scrollView.ViewportWidth, 0.0);
                Verify.AreEqual(scrollView.ViewportHeight, 0.0);
                Verify.AreEqual(scrollView.ScrollableWidth, 0.0);
                Verify.AreEqual(scrollView.ScrollableHeight, 0.0);
            });
        }

        [TestMethod]
        [TestProperty("Description", "Verifies the ScrollView properties after template application.")]
        public void VerifyScrollPresenterAttachedProperties()
        {
            if (PlatformConfiguration.IsOSVersionLessThan(OSVersion.Redstone2))
            {
                Log.Warning("Test is disabled on pre-RS2 because ScrollView not supported pre-RS2");
                return;
            }

            using (PrivateLoggingHelper privateSVLoggingHelper = new PrivateLoggingHelper("ScrollView", "ScrollPresenter"))
            {
                ScrollView scrollView = null;
                Rectangle rectangleScrollViewContent = null;
                AutoResetEvent scrollViewLoadedEvent = new AutoResetEvent(false);
                AutoResetEvent scrollViewUnloadedEvent = new AutoResetEvent(false);

                RunOnUIThread.Execute(() =>
                {
                    rectangleScrollViewContent = new Rectangle();
                    scrollView = new ScrollView();

                    SetupDefaultUI(scrollView, rectangleScrollViewContent, scrollViewLoadedEvent, scrollViewUnloadedEvent);
                });

                WaitForEvent("Waiting for Loaded event", scrollViewLoadedEvent);

                RunOnUIThread.Execute(() =>
                {
                    Log.Comment("Setting ScrollPresenter-cloned properties to non-default values");
                    scrollView.IgnoredInputKind = InputKind.MouseWheel | InputKind.Pen;
                    scrollView.ContentOrientation = ContentOrientation.Horizontal;
                    scrollView.HorizontalScrollChainingMode = ChainingMode.Always;
                    scrollView.VerticalScrollChainingMode = ChainingMode.Never;
                    scrollView.HorizontalScrollRailingMode = RailingMode.Disabled;
                    scrollView.VerticalScrollRailingMode = RailingMode.Disabled;
                    scrollView.HorizontalScrollMode = ScrollMode.Enabled;
                    scrollView.VerticalScrollMode = ScrollMode.Disabled;
                    scrollView.ZoomMode = ZoomMode.Enabled;
                    scrollView.ZoomChainingMode = ChainingMode.Never;
                    scrollView.MinZoomFactor = 2.0;
                    scrollView.MaxZoomFactor = 8.0;

                    Log.Comment("Verifying ScrollPresenter-cloned non-default properties");
                    Verify.AreEqual(scrollView.IgnoredInputKind, InputKind.MouseWheel | InputKind.Pen);
                    Verify.AreEqual(scrollView.ContentOrientation, ContentOrientation.Horizontal);
                    Verify.AreEqual(scrollView.HorizontalScrollChainingMode, ChainingMode.Always);
                    Verify.AreEqual(scrollView.VerticalScrollChainingMode, ChainingMode.Never);
                    Verify.AreEqual(scrollView.HorizontalScrollRailingMode, RailingMode.Disabled);
                    Verify.AreEqual(scrollView.VerticalScrollRailingMode, RailingMode.Disabled);
                    Verify.AreEqual(scrollView.HorizontalScrollMode, ScrollMode.Enabled);
                    Verify.AreEqual(scrollView.VerticalScrollMode, ScrollMode.Disabled);
#if USE_SCROLLMODE_AUTO
                    Verify.AreEqual(scrollView.ComputedHorizontalScrollMode, ScrollMode.Enabled);
                    Verify.AreEqual(scrollView.ComputedVerticalScrollMode, ScrollMode.Disabled);
#endif
                    Verify.AreEqual(scrollView.ZoomMode, ZoomMode.Enabled);
                    Verify.AreEqual(scrollView.ZoomChainingMode, ChainingMode.Never);
                    Verify.IsGreaterThan(scrollView.MinZoomFactor, 2.0 - c_epsilon);
                    Verify.IsLessThan(scrollView.MinZoomFactor, 2.0 + c_epsilon);
                    Verify.IsGreaterThan(scrollView.MaxZoomFactor, 8.0 - c_epsilon);
                    Verify.IsLessThan(scrollView.MaxZoomFactor, 8.0 + c_epsilon);

                    Log.Comment("Resetting window content and ScrollView");
                    Content = null;
                    scrollView = null;
                });

                WaitForEvent("Waiting for Unloaded event", scrollViewUnloadedEvent);

                IdleSynchronizer.Wait();
                Log.Comment("Garbage collecting...");
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                Log.Comment("Done");
            }
        }

        [TestMethod]
        [TestProperty("Description", "Verifies the ScrollPresenter attached properties.")]
        public void VerifyPropertyValuesAfterTemplateApplication()
        {
            if (PlatformConfiguration.IsOSVersionLessThan(OSVersion.Redstone2))
            {
                Log.Warning("Test is disabled on pre-RS2 because ScrollView not supported pre-RS2");
                return;
            }

            using (PrivateLoggingHelper privateSVLoggingHelper = new PrivateLoggingHelper("ScrollView", "ScrollPresenter"))
            {
                ScrollView scrollView = null;
                Rectangle rectangleScrollViewContent = null;
                AutoResetEvent scrollViewLoadedEvent = new AutoResetEvent(false);
                AutoResetEvent scrollViewUnloadedEvent = new AutoResetEvent(false);

                RunOnUIThread.Execute(() =>
                {
                    rectangleScrollViewContent = new Rectangle();
                    scrollView = new ScrollView();

                    SetupDefaultUI(scrollView, rectangleScrollViewContent, scrollViewLoadedEvent, scrollViewUnloadedEvent);
                });

                WaitForEvent("Waiting for Loaded event", scrollViewLoadedEvent);

                RunOnUIThread.Execute(() =>
                {
                    Log.Comment("Verifying ScrollView property values after Loaded event");
                    Verify.AreEqual(scrollView.Content, rectangleScrollViewContent);
                    Verify.IsNotNull(ScrollViewTestHooks.GetScrollPresenterPart(scrollView));
                    Verify.AreEqual(ScrollViewTestHooks.GetScrollPresenterPart(scrollView).Content, rectangleScrollViewContent);
                    Verify.IsNotNull(scrollView.HorizontalScrollController);
                    Verify.IsNotNull(scrollView.VerticalScrollController);
                    Verify.AreEqual(scrollView.ExtentWidth, c_defaultUIScrollViewContentWidth);
                    Verify.AreEqual(scrollView.ExtentHeight, c_defaultUIScrollViewContentHeight);
                    Verify.AreEqual(scrollView.ViewportWidth, c_defaultUIScrollViewWidth);
                    Verify.AreEqual(scrollView.ViewportHeight, c_defaultUIScrollViewHeight);
                    Verify.AreEqual(scrollView.ScrollableWidth, c_defaultUIScrollViewContentWidth - c_defaultUIScrollViewWidth);
                    Verify.AreEqual(scrollView.ScrollableHeight, c_defaultUIScrollViewContentHeight - c_defaultUIScrollViewHeight);

                    Log.Comment("Resetting window content and ScrollView");
                    Content = null;
                    scrollView = null;
                });

                WaitForEvent("Waiting for Unloaded event", scrollViewUnloadedEvent);

                IdleSynchronizer.Wait();
                Log.Comment("Garbage collecting...");
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                Log.Comment("Done");
            }
        }

        [TestMethod]
        [TestProperty("Description", "Verifies the ScrollView visual state changes based on the AutoHideScrollBars, IsEnabled and ScrollBarVisibility settings.")]
        public void VerifyVisualStates()
        {
            UISettings settings = new UISettings();
            if (!settings.AnimationsEnabled)
            {
                Log.Warning("Test is disabled when animations are turned off.");
                return;
            }

            VerifyVisualStates(ScrollBarVisibility.Auto, autoHideScrollControllers: true);
            VerifyVisualStates(ScrollBarVisibility.Visible, autoHideScrollControllers: true);

            // Non-auto-hiding ScrollControllers are only supported starting with RS4.
            if (PlatformConfiguration.IsOsVersionGreaterThanOrEqual(OSVersion.Redstone4))
            {
                VerifyVisualStates(ScrollBarVisibility.Auto, autoHideScrollControllers: false);
                VerifyVisualStates(ScrollBarVisibility.Visible, autoHideScrollControllers: false);
            }
        }

        private void VerifyVisualStates(ScrollBarVisibility scrollBarVisibility, bool autoHideScrollControllers)
        {
            using (PrivateLoggingHelper privateSVLoggingHelper = new PrivateLoggingHelper("ScrollView"))
            {
                ScrollView scrollView = null;

                RunOnUIThread.Execute(() =>
                {
                    MUXControlsTestHooks.LoggingMessage += MUXControlsTestHooks_LoggingMessageForVisualStateChange;
                    m_scrollViewVisualStateCounts = new ScrollViewVisualStateCounts();
                    scrollView = new ScrollView();
                });

                using (ScrollViewTestHooksHelper scrollViewTestHooksHelper = new ScrollViewTestHooksHelper(scrollView, autoHideScrollControllers))
                {
                    Rectangle rectangleScrollViewContent = null;
                    AutoResetEvent scrollViewLoadedEvent = new AutoResetEvent(false);
                    AutoResetEvent scrollViewUnloadedEvent = new AutoResetEvent(false);

                    RunOnUIThread.Execute(() =>
                    {
                        rectangleScrollViewContent = new Rectangle();
                        scrollView.HorizontalScrollBarVisibility = scrollBarVisibility;
                        scrollView.VerticalScrollBarVisibility = scrollBarVisibility;

                        SetupDefaultUI(
                            scrollView: scrollView,
                            rectangleScrollViewContent: rectangleScrollViewContent,
                            scrollViewLoadedEvent: scrollViewLoadedEvent,
                            scrollViewUnloadedEvent: scrollViewUnloadedEvent,
                            setAsContentRoot: true,
                            useParentGrid: true);
                    });

                    WaitForEvent("Waiting for Loaded event", scrollViewLoadedEvent);

                    RunOnUIThread.Execute(() =>
                    {
                        MUXControlsTestHooks.LoggingMessage -= MUXControlsTestHooks_LoggingMessageForVisualStateChange;
                        Log.Comment($"VerifyVisualStates: isEnabled:True, scrollBarVisibility:{scrollBarVisibility}, autoHideScrollControllers:{autoHideScrollControllers}");

                        VerifyVisualStates(
                            expectedMouseIndicatorStateCount: autoHideScrollControllers ? 0u : (scrollBarVisibility == ScrollBarVisibility.Auto ? 2u : 3u),
                            expectedTouchIndicatorStateCount: 0,
                            expectedNoIndicatorStateCount: autoHideScrollControllers ? (scrollBarVisibility == ScrollBarVisibility.Auto ? 2u : 3u) : 0u,
                            expectedScrollBarsSeparatorCollapsedStateCount: autoHideScrollControllers ? (scrollBarVisibility == ScrollBarVisibility.Auto ? 1u : 3u) : 0u,
                            expectedScrollBarsSeparatorCollapsedDisabledStateCount: 0,
                            expectedScrollBarsSeparatorExpandedStateCount: autoHideScrollControllers ? 0u : (scrollBarVisibility == ScrollBarVisibility.Auto ? 1u : 3u),
                            expectedScrollBarsSeparatorDisplayedWithoutAnimationStateCount: 0,
                            expectedScrollBarsSeparatorExpandedWithoutAnimationStateCount: 0,
                            expectedScrollBarsSeparatorCollapsedWithoutAnimationStateCount: 0);

                        m_scrollViewVisualStateCounts.ResetStateCounts();
                        MUXControlsTestHooks.LoggingMessage += MUXControlsTestHooks_LoggingMessageForVisualStateChange;

                        Log.Comment("Disabling ScrollView");
                        scrollView.IsEnabled = false;
                    });

                    IdleSynchronizer.Wait();

                    RunOnUIThread.Execute(() =>
                    {
                        MUXControlsTestHooks.LoggingMessage -= MUXControlsTestHooks_LoggingMessageForVisualStateChange;
                        Log.Comment($"VerifyVisualStates: isEnabled:False, scrollBarVisibility:{scrollBarVisibility}, autoHideScrollControllers:{autoHideScrollControllers}");

                        VerifyVisualStates(
                            expectedMouseIndicatorStateCount: autoHideScrollControllers ? 0u : (scrollBarVisibility == ScrollBarVisibility.Auto ? 1u : 3u),
                            expectedTouchIndicatorStateCount: 0,
                            expectedNoIndicatorStateCount: autoHideScrollControllers ? (scrollBarVisibility == ScrollBarVisibility.Auto ? 1u : 3u) : 0u,
                            expectedScrollBarsSeparatorCollapsedStateCount: 0,
                            expectedScrollBarsSeparatorCollapsedDisabledStateCount: scrollBarVisibility == ScrollBarVisibility.Auto ? 0u : 3u,
                            expectedScrollBarsSeparatorExpandedStateCount: 0,
                            expectedScrollBarsSeparatorDisplayedWithoutAnimationStateCount: 0,
                            expectedScrollBarsSeparatorExpandedWithoutAnimationStateCount: 0,
                            expectedScrollBarsSeparatorCollapsedWithoutAnimationStateCount: 0);

                        m_scrollViewVisualStateCounts.ResetStateCounts();
                        MUXControlsTestHooks.LoggingMessage += MUXControlsTestHooks_LoggingMessageForVisualStateChange;

                        Log.Comment("Enabling ScrollView");
                        scrollView.IsEnabled = true;
                    });

                    IdleSynchronizer.Wait();

                    RunOnUIThread.Execute(() =>
                    {
                        MUXControlsTestHooks.LoggingMessage -= MUXControlsTestHooks_LoggingMessageForVisualStateChange;
                        Log.Comment($"VerifyVisualStates: isEnabled:True, scrollBarVisibility:{scrollBarVisibility}, autoHideScrollControllers:{autoHideScrollControllers}");

                        VerifyVisualStates(
                            expectedMouseIndicatorStateCount: autoHideScrollControllers ? 0u : 3u,
                            expectedTouchIndicatorStateCount: 0,
                            expectedNoIndicatorStateCount: autoHideScrollControllers ? 3u : 0u,
                            expectedScrollBarsSeparatorCollapsedStateCount: autoHideScrollControllers ? (scrollBarVisibility == ScrollBarVisibility.Auto ? 2u : 3u) : 0u,
                            expectedScrollBarsSeparatorCollapsedDisabledStateCount: 0,
                            expectedScrollBarsSeparatorExpandedStateCount: autoHideScrollControllers ? 0u : (scrollBarVisibility == ScrollBarVisibility.Auto ? 2u : 3u),
                            expectedScrollBarsSeparatorDisplayedWithoutAnimationStateCount: 0,
                            expectedScrollBarsSeparatorExpandedWithoutAnimationStateCount: 0,
                            expectedScrollBarsSeparatorCollapsedWithoutAnimationStateCount: 0);

                        Log.Comment("Resetting window content");
                        Content = null;
                        m_scrollViewVisualStateCounts = null;
                    });

                    WaitForEvent("Waiting for Unloaded event", scrollViewUnloadedEvent);
                }
            }
        }

        private void MUXControlsTestHooks_LoggingMessageForVisualStateChange(object sender, MUXControlsTestHooksLoggingMessageEventArgs args)
        {
            if (args.IsVerboseLevel)
            {
                if (args.Message.Contains("ScrollView::GoToState"))
                {
                    if (args.Message.Contains("NoIndicator"))
                    {
                        m_scrollViewVisualStateCounts.NoIndicatorStateCount++;
                    }
                    else if (args.Message.Contains("TouchIndicator"))
                    {
                        m_scrollViewVisualStateCounts.TouchIndicatorStateCount++;
                    }
                    else if (args.Message.Contains("MouseIndicator"))
                    {
                        m_scrollViewVisualStateCounts.MouseIndicatorStateCount++;
                    }
                    else if (args.Message.Contains("ScrollBarsSeparatorCollapsedDisabled"))
                    {
                        m_scrollViewVisualStateCounts.ScrollBarsSeparatorCollapsedDisabledStateCount++;
                    }
                    else if (args.Message.Contains("ScrollBarsSeparatorCollapsedWithoutAnimation"))
                    {
                        m_scrollViewVisualStateCounts.ScrollBarsSeparatorCollapsedWithoutAnimationStateCount++;
                    }
                    else if (args.Message.Contains("ScrollBarsSeparatorDisplayedWithoutAnimation"))
                    {
                        m_scrollViewVisualStateCounts.ScrollBarsSeparatorDisplayedWithoutAnimationStateCount++;
                    }
                    else if (args.Message.Contains("ScrollBarsSeparatorExpandedWithoutAnimation"))
                    {
                        m_scrollViewVisualStateCounts.ScrollBarsSeparatorExpandedWithoutAnimationStateCount++;
                    }
                    else if (args.Message.Contains("ScrollBarsSeparatorCollapsed"))
                    {
                        m_scrollViewVisualStateCounts.ScrollBarsSeparatorCollapsedStateCount++;
                    }
                    else if (args.Message.Contains("ScrollBarsSeparatorExpanded"))
                    {
                        m_scrollViewVisualStateCounts.ScrollBarsSeparatorExpandedStateCount++;
                    }
                }
            }
        }

        private void VerifyVisualStates(
            uint expectedMouseIndicatorStateCount,
            uint expectedTouchIndicatorStateCount,
            uint expectedNoIndicatorStateCount,
            uint expectedScrollBarsSeparatorCollapsedStateCount,
            uint expectedScrollBarsSeparatorCollapsedDisabledStateCount,
            uint expectedScrollBarsSeparatorExpandedStateCount,
            uint expectedScrollBarsSeparatorDisplayedWithoutAnimationStateCount,
            uint expectedScrollBarsSeparatorExpandedWithoutAnimationStateCount,
            uint expectedScrollBarsSeparatorCollapsedWithoutAnimationStateCount)
        {
            Log.Comment($"expectedMouseIndicatorStateCount:{expectedMouseIndicatorStateCount}, mouseIndicatorStateCount:{m_scrollViewVisualStateCounts.MouseIndicatorStateCount}");
            Log.Comment($"expectedNoIndicatorStateCount:{expectedNoIndicatorStateCount}, noIndicatorStateCount:{m_scrollViewVisualStateCounts.NoIndicatorStateCount}");
            Log.Comment($"expectedScrollBarsSeparatorCollapsedStateCount:{expectedScrollBarsSeparatorCollapsedStateCount}, scrollBarsSeparatorCollapsedStateCount:{m_scrollViewVisualStateCounts.ScrollBarsSeparatorCollapsedStateCount}");
            Log.Comment($"expectedScrollBarsSeparatorCollapsedDisabledStateCount:{expectedScrollBarsSeparatorCollapsedDisabledStateCount}, scrollBarsSeparatorCollapsedDisabledStateCount:{m_scrollViewVisualStateCounts.ScrollBarsSeparatorCollapsedDisabledStateCount}");
            Log.Comment($"expectedScrollBarsSeparatorExpandedStateCount:{expectedScrollBarsSeparatorExpandedStateCount}, scrollBarsSeparatorExpandedStateCount:{m_scrollViewVisualStateCounts.ScrollBarsSeparatorExpandedStateCount}");

            Verify.AreEqual(expectedMouseIndicatorStateCount, m_scrollViewVisualStateCounts.MouseIndicatorStateCount);
            Verify.AreEqual(expectedTouchIndicatorStateCount, m_scrollViewVisualStateCounts.TouchIndicatorStateCount);
            Verify.AreEqual(expectedNoIndicatorStateCount, m_scrollViewVisualStateCounts.NoIndicatorStateCount);
            Verify.AreEqual(expectedScrollBarsSeparatorCollapsedStateCount, m_scrollViewVisualStateCounts.ScrollBarsSeparatorCollapsedStateCount);
            Verify.AreEqual(expectedScrollBarsSeparatorCollapsedDisabledStateCount, m_scrollViewVisualStateCounts.ScrollBarsSeparatorCollapsedDisabledStateCount);
            Verify.AreEqual(expectedScrollBarsSeparatorExpandedStateCount, m_scrollViewVisualStateCounts.ScrollBarsSeparatorExpandedStateCount);
            Verify.AreEqual(expectedScrollBarsSeparatorDisplayedWithoutAnimationStateCount, m_scrollViewVisualStateCounts.ScrollBarsSeparatorDisplayedWithoutAnimationStateCount);
            Verify.AreEqual(expectedScrollBarsSeparatorExpandedWithoutAnimationStateCount, m_scrollViewVisualStateCounts.ScrollBarsSeparatorExpandedWithoutAnimationStateCount);
            Verify.AreEqual(expectedScrollBarsSeparatorCollapsedWithoutAnimationStateCount, m_scrollViewVisualStateCounts.ScrollBarsSeparatorCollapsedWithoutAnimationStateCount);
        }

        [TestMethod]
        [TestProperty("Description", "Verifies anchor candidate registration and unregistration.")]
        public void VerifyAnchorCandidateRegistration()
        {
            if (PlatformConfiguration.IsOSVersionLessThan(OSVersion.Redstone2))
            {
                Log.Warning("Test is disabled on pre-RS2 because ScrollView not supported pre-RS2");
                return;
            }

            using (PrivateLoggingHelper privateSVLoggingHelper = new PrivateLoggingHelper("ScrollView", "ScrollPresenter"))
            {
                int expectedAnchorCandidatesCount = 0;
                ScrollPresenter scrollPresenter = null;
                ScrollView scrollView = null;
                Rectangle rectangleScrollViewContent = null;
                AutoResetEvent scrollViewLoadedEvent = new AutoResetEvent(false);
                AutoResetEvent scrollViewAnchorRequestedEvent = new AutoResetEvent(false);

                RunOnUIThread.Execute(() =>
                {
                    rectangleScrollViewContent = new Rectangle();
                    scrollView = new ScrollView();
                    scrollView.HorizontalAnchorRatio = 0.1;

                    SetupDefaultUI(scrollView, rectangleScrollViewContent, scrollViewLoadedEvent);

                    scrollView.AnchorRequested += (ScrollView sender, ScrollingAnchorRequestedEventArgs args) =>
                    {
                        Log.Comment("ScrollView.AnchorRequested event handler. args.AnchorCandidates.Count: " + args.AnchorCandidates.Count);
                        Verify.IsNull(args.AnchorElement);
                        Verify.AreEqual(expectedAnchorCandidatesCount, args.AnchorCandidates.Count);
                        scrollViewAnchorRequestedEvent.Set();
                    };
                });

                WaitForEvent("Waiting for Loaded event", scrollViewLoadedEvent);

                RunOnUIThread.Execute(() =>
                {
                    Log.Comment("Accessing inner ScrollPresenter control");
                    scrollPresenter = ScrollViewTestHooks.GetScrollPresenterPart(scrollView);

                    Log.Comment("Registering Rectangle as anchor candidate");
                    scrollView.RegisterAnchorCandidate(rectangleScrollViewContent);
                    expectedAnchorCandidatesCount = 1;

                    Log.Comment("Forcing ScrollPresenter layout");
                    scrollPresenter.InvalidateArrange();
                });

                WaitForEvent("Waiting for AnchorRequested event", scrollViewAnchorRequestedEvent);

                RunOnUIThread.Execute(() =>
                {
                    Log.Comment("Unregistering Rectangle as anchor candidate");
                    scrollView.UnregisterAnchorCandidate(rectangleScrollViewContent);
                    expectedAnchorCandidatesCount = 0;

                    Log.Comment("Forcing ScrollPresenter layout");
                    scrollPresenter.InvalidateArrange();
                });

                WaitForEvent("Waiting for AnchorRequested event", scrollViewAnchorRequestedEvent);
            }
        }

        private void SetupDefaultUI(
            ScrollView scrollView,
            Rectangle rectangleScrollViewContent = null,
            AutoResetEvent scrollViewLoadedEvent = null,
            AutoResetEvent scrollViewUnloadedEvent = null,
            bool setAsContentRoot = true,
            bool useParentGrid = false)
        {
            Log.Comment("Setting up default UI with ScrollView" + (rectangleScrollViewContent == null ? "" : " and Rectangle"));

            LinearGradientBrush twoColorLGB = new LinearGradientBrush() { StartPoint = new Point(0, 0), EndPoint = new Point(1, 1) };

            GradientStop brownGS = new GradientStop() { Color = Colors.Brown, Offset = 0.0 };
            twoColorLGB.GradientStops.Add(brownGS);

            GradientStop orangeGS = new GradientStop() { Color = Colors.Orange, Offset = 1.0 };
            twoColorLGB.GradientStops.Add(orangeGS);

            if (rectangleScrollViewContent != null)
            {
                rectangleScrollViewContent.Width = c_defaultUIScrollViewContentWidth;
                rectangleScrollViewContent.Height = c_defaultUIScrollViewContentHeight;
                rectangleScrollViewContent.Fill = twoColorLGB;
            }

            Verify.IsNotNull(scrollView);
            scrollView.Name = "scrollView";
            scrollView.Width = c_defaultUIScrollViewWidth;
            scrollView.Height = c_defaultUIScrollViewHeight;
            if (rectangleScrollViewContent != null)
            {
                scrollView.Content = rectangleScrollViewContent;
            }

            if (scrollViewLoadedEvent != null)
            {
                scrollView.Loaded += (object sender, RoutedEventArgs e) =>
                {
                    Log.Comment("ScrollView.Loaded event handler");
                    scrollViewLoadedEvent.Set();
                };
            }

            if (scrollViewUnloadedEvent != null)
            {
                scrollView.Unloaded += (object sender, RoutedEventArgs e) =>
                {
                    Log.Comment("ScrollView.Unloaded event handler");
                    scrollViewUnloadedEvent.Set();
                };
            }

            Grid parentGrid = null;

            if (useParentGrid)
            {
                parentGrid = new Grid();
                parentGrid.Width = c_defaultUIScrollViewWidth * 3;
                parentGrid.Height = c_defaultUIScrollViewHeight * 3;

                scrollView.HorizontalAlignment = HorizontalAlignment.Left;
                scrollView.VerticalAlignment = VerticalAlignment.Top;

                parentGrid.Children.Add(scrollView);
            }

            if (setAsContentRoot)
            {
                Log.Comment("Setting window content");
                if (useParentGrid)
                {
                    Content = parentGrid;
                }
                else
                {
                    Content = scrollView;
                }
            }
        }

        private void WaitForEvent(string logComment, EventWaitHandle eventWaitHandle)
        {
            Log.Comment(logComment);
            if (!eventWaitHandle.WaitOne(TimeSpan.FromMilliseconds(c_MaxWaitDuration)))
            {
                throw new Exception("Timeout expiration in WaitForEvent.");
            }
        }
    }

    // Custom ScrollView that records its visual state changes.
    public class ScrollViewVisualStateCounts
    {
        public uint NoIndicatorStateCount
        {
            get;
            set;
        }

        public uint TouchIndicatorStateCount
        {
            get;
            set;
        }

        public uint MouseIndicatorStateCount
        {
            get;
            set;
        }

        public uint ScrollBarsSeparatorExpandedStateCount
        {
            get;
            set;
        }

        public uint ScrollBarsSeparatorCollapsedStateCount
        {
            get;
            set;
        }

        public uint ScrollBarsSeparatorCollapsedDisabledStateCount
        {
            get;
            set;
        }

        public uint ScrollBarsSeparatorCollapsedWithoutAnimationStateCount
        {
            get;
            set;
        }

        public uint ScrollBarsSeparatorDisplayedWithoutAnimationStateCount
        {
            get;
            set;
        }

        public uint ScrollBarsSeparatorExpandedWithoutAnimationStateCount
        {
            get;
            set;
        }

        public void ResetStateCounts()
        {
            NoIndicatorStateCount = 0;
            TouchIndicatorStateCount = 0;
            MouseIndicatorStateCount = 0;
            ScrollBarsSeparatorExpandedStateCount = 0;
            ScrollBarsSeparatorCollapsedStateCount = 0;
            ScrollBarsSeparatorCollapsedDisabledStateCount = 0;
            ScrollBarsSeparatorCollapsedWithoutAnimationStateCount = 0;
            ScrollBarsSeparatorDisplayedWithoutAnimationStateCount = 0;
            ScrollBarsSeparatorExpandedWithoutAnimationStateCount = 0;
        }
    }
}
