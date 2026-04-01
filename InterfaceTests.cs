// MIT License
//
// Copyright (c) 2026 BruceMellows
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using BruceMellows.MVVM.ViewModel.Proxy;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;

namespace MVVM.ViewModel.Proxy.Tests;

public sealed class InterfaceTests
{
	public enum ViewModelType
	{
		WithOnChanged,
		WithOnChanging,
		WithOnChangingAndChanged,
		WithPropertyOnly,
		WithPropertyOnlyInterface,
	}

	[Test]
	public void GetViewModelProxy([Values] ViewModelType viewModelType)
	{
		// Act/Assert
		Assert.DoesNotThrow(() => GetViewModelProxyImplementation(viewModelType));
	}

	[Test]
	public void GetViewModel([Values] ViewModelType viewModelType)
	{
		// Arrange
		var proxy = GetViewModelProxyImplementation(viewModelType);

		// Act/Assert
		Assert.IsNotNull(GetViewModel(proxy));
	}

	[Test]
	public void HasTargetProperty([Values] ViewModelType viewModelType)
	{
		// Arrange
		var proxy = GetViewModelProxyImplementation(viewModelType);
		var vm = GetViewModel(proxy);

		// Act
		var namePropertyInfo = GetPropertyInfo(vm.GetType(), "Name");

		// Assert
		Assert.That(namePropertyInfo, Is.Not.Null);
		Assert.Multiple(() =>
		{
			Assert.That(namePropertyInfo.GetGetMethod(), Is.Not.Null);
			Assert.That(namePropertyInfo.GetSetMethod(), Is.Not.Null);
		});
	}

	[Test]
	public void GetUnsetProperty([Values] ViewModelType viewModelType)
	{
		// Arrange
		var proxy = GetViewModelProxyImplementation(viewModelType);
		var vm = GetViewModel(proxy);
		var namePropertyInfo = GetPropertyInfo(vm.GetType(), "Name");

		// Act/Assert
		var ex = Assert.Throws<TargetInvocationException>(() => namePropertyInfo.GetGetMethod()!.Invoke(vm, []));
		Assert.That(ex.InnerException, Is.Not.Null);
		Assert.That(ex.InnerException, Is.TypeOf<NoInitialValueException>());
	}

	[Test]
	public void SetProperty([Values] ViewModelType viewModelType)
	{
		// Arrange
		var proxy = GetViewModelProxyImplementation(viewModelType);
		var vm = GetViewModel(proxy);
		var namePropertyInfo = GetPropertyInfo(vm.GetType(), "Name");

		// Act/Assert
		Assert.DoesNotThrow(() => namePropertyInfo.GetSetMethod()!.Invoke(vm, [string.Empty]));
	}

	[Test]
	public void GetSetProperty([Values] ViewModelType viewModelType)
	{
		// Arrange
		var expected = Guid.NewGuid().ToString();
		var proxy = GetViewModelProxyImplementation(viewModelType);
		var vm = GetViewModel(proxy);
		var namePropertyInfo = GetPropertyInfo(vm.GetType(), "Name");
		namePropertyInfo.GetSetMethod()!.Invoke(vm, [expected]);

		// Act
		var actual = namePropertyInfo.GetGetMethod()!.Invoke(vm, []);

		//Assert
		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void OncePropertyChanging()
	{
		// Arrange
		var proxy = new ViewModelProxyWithOnChanging();
		var vm = proxy.Proxied;
		var callCount = 0;
		vm.PropertyChanging += (s, e) =>
		{
			Assert.That(e.PropertyName, Is.EqualTo("Name"));
			Interlocked.Increment(ref callCount);
		};

		// Act
		vm.Name = "Test";

		// Assert
		Assert.That(callCount, Is.EqualTo(1));
	}

	[Test]
	public void TwicePropertyChanging()
	{
		// Arrange
		var proxy = new ViewModelProxyWithOnChanging();
		var vm = proxy.Proxied;
		var callCount = 0;
		vm.PropertyChanging += (s, e) =>
		{
			Assert.That(e.PropertyName, Is.EqualTo("Name"));
			Interlocked.Increment(ref callCount);
		};

		// Act
		vm.Name = "Test";
		vm.Name = "Test";

		// Assert
		Assert.That(callCount, Is.EqualTo(2));
	}

	[Test]
	public void OncePropertyChanged()
	{
		// Arrange
		var proxy = new ViewModelProxyWithOnChanged();
		var vm = proxy.Proxied;
		var callCount = 0;
		vm.PropertyChanged += (s, e) =>
		{
			Assert.That(e.PropertyName, Is.EqualTo("Name"));
			Interlocked.Increment(ref callCount);
		};

		// Act
		vm.Name = "Test";

		// Assert
		Assert.That(callCount, Is.EqualTo(1));
	}

	[Test]
	public void TwicePropertyChanged()
	{
		// Arrange
		var proxy = new ViewModelProxyWithOnChanged();
		var vm = proxy.Proxied;
		var callCount = 0;
		vm.PropertyChanged += (s, e) =>
		{
			Assert.That(e.PropertyName, Is.EqualTo("Name"));
			Interlocked.Increment(ref callCount);
		};

		// Act
		vm.Name = "Test";
		vm.Name = "Test";

		// Assert
		Assert.That(callCount, Is.EqualTo(1));
	}

	[Test]
	public void OncePropertyChangingAndChanged()
	{
		// Arrange
		var proxy = new ViewModelProxyWithOnChangingAndChanged();
		var vm = proxy.Proxied;
		var changingCallCount = 0;
		var changedCallCount = 0;
		vm.PropertyChanged += (s, e) =>
		{
			Assert.That(e.PropertyName, Is.EqualTo("Name"));
			Interlocked.Increment(ref changedCallCount);
		};

		vm.PropertyChanging += (s, e) =>
		{
			Assert.That(e.PropertyName, Is.EqualTo("Name"));
			Interlocked.Increment(ref changingCallCount);
		};

		// Act
		vm.Name = "Test";

		// Assert
		Assert.That(changingCallCount, Is.EqualTo(1));
		Assert.That(changedCallCount, Is.EqualTo(1));
	}

	[Test]
	public void TwicePropertyChangingAndChanged()
	{
		// Arrange
		var proxy = new ViewModelProxyWithOnChangingAndChanged();
		var vm = proxy.Proxied;
		var changingCallCount = 0;
		var changedCallCount = 0;
		vm.PropertyChanged += (s, e) =>
		{
			Assert.That(e.PropertyName, Is.EqualTo("Name"));
			Interlocked.Increment(ref changedCallCount);
		};

		vm.PropertyChanging += (s, e) =>
		{
			Assert.That(e.PropertyName, Is.EqualTo("Name"));
			Interlocked.Increment(ref changingCallCount);
		};

		// Act
		vm.Name = "Test";
		vm.Name = "Test";

		// Assert
		Assert.That(changingCallCount, Is.EqualTo(2));
		Assert.That(changedCallCount, Is.EqualTo(1));
	}

	[Test]
	public void ViewModelAndProxyWithDifferentEventingInterfaces()
	{
		// Arrange
		var proxy = new ViewModelProxyWithDifferentEventingInterfaces();

		// Act/Assert
		Assert.Throws<MissingEventInterfaceException>(() => proxy.Proxied.CollectionChanged += (s, e) => { });
	}

	[Test]
	public void InterfaceWithMethod()
	{
		// Act/Assert
		var targetInvocationException = Assert.Throws<TargetInvocationException>(() => new ViewModelWithMethod());
		Assert.That(targetInvocationException.InnerException, Is.TypeOf<InvalidTargetTypeException>());
	}

	private static object GetViewModel(object proxy)
	{
		var viewModelPropertyInfo = proxy.GetType().GetProperty("Proxied", BindingFlags.Public | BindingFlags.Instance);
		if (viewModelPropertyInfo is null)
		{
			throw new InvalidOperationException($"Property 'Proxied' not found in type '{proxy.GetType().FullName}'.");
		}
		return viewModelPropertyInfo.GetGetMethod()!.Invoke(proxy, [])!;
	}

	private static object GetViewModelProxyImplementation(ViewModelType viewModelType)
	{
		return GetViewModelProxyType(viewModelType)
			.GetConstructor(Type.EmptyTypes)
			?.Invoke(null)
			?? throw new NotImplementedException();
	}

	private static Type GetViewModelProxyType(ViewModelType viewModelType)
	{
		return viewModelType switch
		{
			ViewModelType.WithOnChanged => typeof(ViewModelProxyWithOnChanged),
			ViewModelType.WithOnChanging => typeof(ViewModelProxyWithOnChanging),
			ViewModelType.WithOnChangingAndChanged => typeof(ViewModelProxyWithOnChangingAndChanged),
			ViewModelType.WithPropertyOnly => typeof(ViewModelProxyWithPropertyOnly),
			ViewModelType.WithPropertyOnlyInterface => typeof(ViewModelWithPropertyOnlyInterface),
			_ => throw new ArgumentOutOfRangeException(nameof(viewModelType), viewModelType, null)
		};
	}

	private static PropertyInfo GetPropertyInfo(Type type, string propertyName)
	{
		var propertyInfo = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
		if (propertyInfo is null)
		{
			throw new InvalidOperationException($"Property '{propertyName}' not found in type '{type.FullName}'.");
		}
		return propertyInfo;
	}

	#region ITestViewModel
	public interface IViewModelWithPropertyOnly
	{
		string Name { get; set; }
	}

	public interface IViewModelWithOnChanged : INotifyPropertyChanged
	{
		string Name { get; set; }
	}

	public interface IViewModelWithOnChanging : INotifyPropertyChanging
	{
		string Name { get; set; }
	}

	public interface IViewModelWithOnChangingAndChanged : INotifyPropertyChanging, INotifyPropertyChanged
	{
		string Name { get; set; }
	}

	public interface IViewModelWithPropertyOnlyInterface : IViewModelWithPropertyOnly
	{
	}

	public interface IViewModelProxyWithDifferentEventingInterfaces : INotifyCollectionChanged
	{
		string Name { get; set; }
	}

	public interface IViewModelWithMethod
	{
		void Method();
	}
	#endregion ITestViewModel

	#region ITestViewModelProxy
	public sealed class ViewModelProxyWithPropertyOnly : ViewModelProxy<IViewModelWithPropertyOnly>
	{
	}

	public sealed class ViewModelProxyWithOnChanged : ViewModelProxy<IViewModelWithOnChanged>, INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;
	}

	public sealed class ViewModelProxyWithOnChanging : ViewModelProxy<IViewModelWithOnChanging>, INotifyPropertyChanging
	{
		public event PropertyChangingEventHandler? PropertyChanging;
	}

	public sealed class ViewModelProxyWithOnChangingAndChanged : ViewModelProxy<IViewModelWithOnChangingAndChanged>, INotifyPropertyChanging, INotifyPropertyChanged
	{
		public event PropertyChangingEventHandler? PropertyChanging;
		public event PropertyChangedEventHandler? PropertyChanged;
	}

	public sealed class ViewModelWithPropertyOnlyInterface : ViewModelProxy<IViewModelWithPropertyOnlyInterface>
	{
	}

	public sealed class ViewModelWithMethod : ViewModelProxy<IViewModelWithMethod>
	{
	}

	public sealed class ViewModelProxyWithDifferentEventingInterfaces : ViewModelProxy<IViewModelProxyWithDifferentEventingInterfaces>
	{
	}
	#endregion ITestViewModelProcy
}
