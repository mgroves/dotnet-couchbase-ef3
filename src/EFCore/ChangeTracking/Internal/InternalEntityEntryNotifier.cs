// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped"/>. This means that each
    ///         <see cref="DbContext"/> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public class InternalEntityEntryNotifier : IInternalEntityEntryNotifier
    {
        private readonly IQueryTrackingListener[] _queryTrackingListeners;
        private readonly IEntityStateListener[] _entityStateListeners;
        private readonly IPropertyListener[] _propertyListeners;
        private readonly INavigationListener[] _navigationListeners;
        private readonly IKeyListener[] _keyListeners;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public InternalEntityEntryNotifier(
            [CanBeNull] IEnumerable<IEntityStateListener> entityStateListeners,
            [CanBeNull] IEnumerable<IPropertyListener> propertyListeners,
            [CanBeNull] IEnumerable<INavigationListener> navigationListeners,
            [CanBeNull] IEnumerable<IKeyListener> keyListeners,
            [CanBeNull] IEnumerable<IQueryTrackingListener> queryTrackingListeners)
        {
            if (entityStateListeners != null)
            {
                var listeners = entityStateListeners.ToArray();
                _entityStateListeners = listeners.Length == 0 ? null : listeners;
            }

            if (propertyListeners != null)
            {
                var listeners = propertyListeners.ToArray();
                _propertyListeners = listeners.Length == 0 ? null : listeners;
            }

            if (navigationListeners != null)
            {
                var listeners = navigationListeners.ToArray();
                _navigationListeners = listeners.Length == 0 ? null : listeners;
            }

            if (keyListeners != null)
            {
                var listeners = keyListeners.ToArray();
                _keyListeners = listeners.Length == 0 ? null : listeners;
            }

            if (queryTrackingListeners != null)
            {
                var listeners = queryTrackingListeners.ToArray();
                _queryTrackingListeners = listeners.Length == 0 ? null : listeners;
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void StateChanging(InternalEntityEntry entry, EntityState newState)
        {
            if (_entityStateListeners == null)
            {
                return;
            }

            foreach (var listener in _entityStateListeners)
            {
                listener.StateChanging(entry, newState);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void StateChanged(InternalEntityEntry entry, EntityState oldState, bool fromQuery)
        {
            if (_entityStateListeners == null)
            {
                return;
            }

            foreach (var listener in _entityStateListeners)
            {
                listener.StateChanged(entry, oldState, fromQuery);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void TrackedFromQuery(
            InternalEntityEntry entry,
            ISet<IForeignKey> handledForeignKeys)
        {
            if (_entityStateListeners == null)
            {
                return;
            }

            foreach (var listener in _queryTrackingListeners)
            {
                listener.TrackedFromQuery(entry, handledForeignKeys);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void NavigationReferenceChanged(
            InternalEntityEntry entry,
            INavigation navigation,
            object oldValue,
            object newValue)
        {
            if (_navigationListeners == null)
            {
                return;
            }

            foreach (var listener in _navigationListeners)
            {
                listener.NavigationReferenceChanged(entry, navigation, oldValue, newValue);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void NavigationCollectionChanged(
            InternalEntityEntry entry,
            INavigation navigation,
            IEnumerable<object> added,
            IEnumerable<object> removed)
        {
            if (_navigationListeners == null)
            {
                return;
            }

            foreach (var listener in _navigationListeners)
            {
                listener.NavigationCollectionChanged(entry, navigation, added, removed);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void KeyPropertyChanged(
            InternalEntityEntry entry,
            IProperty property,
            IReadOnlyList<IKey> keys,
            IReadOnlyList<IForeignKey> foreignKeys,
            object oldValue,
            object newValue)
        {
            if (_keyListeners == null)
            {
                return;
            }

            foreach (var listener in _keyListeners)
            {
                listener.KeyPropertyChanged(entry, property, keys, foreignKeys, oldValue, newValue);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void PropertyChanged(InternalEntityEntry entry, IPropertyBase property, bool setModified)
        {
            if (_propertyListeners == null)
            {
                return;
            }

            foreach (var listener in _propertyListeners)
            {
                listener.PropertyChanged(entry, property, setModified);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void PropertyChanging(InternalEntityEntry entry, IPropertyBase property)
        {
            if (_propertyListeners == null)
            {
                return;
            }

            foreach (var listener in _propertyListeners)
            {
                listener.PropertyChanging(entry, property);
            }
        }
    }
}
