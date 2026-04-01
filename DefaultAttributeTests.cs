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

namespace MVVM.ViewModel.Proxy.Tests;

public sealed class DefaultAttributeTests
{
	[Test]
	public void DefaultConstantString()
	{
		// Arrange
		var proxy = new TestViewModelProxy();
		var vm = proxy.Proxied;

		// Act/Assert
		Assert.That(vm.Test, Is.EqualTo("test"));
	}

	[Test]
	public void DefaultConstantsInt()
	{
		// Arrange
		var proxy = new TestViewModelProxy();
		var vm = proxy.Proxied;

		// Act/Assert
		Assert.That(vm.Zero, Is.EqualTo(0));
	}

	[Test]
	public void DefaultConstantDouble()
	{
		// Arrange
		var proxy = new TestViewModelProxy();
		var vm = proxy.Proxied;

		// Act/Assert
		Assert.That(vm.OnePointFive, Is.EqualTo(1.5));
	}

	[Test]
	public void DefaultConvertedDateTimeConverter()
	{
		// Arrange
		var proxy = new TestViewModelProxy();
		var vm = proxy.Proxied;

		// Act/Assert
		Assert.That(vm.EpochParty, Is.EqualTo(new DateTime(1999, 12, 31)));
	}

	public interface ITestViewModel
	{
		[DefaultConstant("test")]
		string Test { get; set; }

		[DefaultConstant(0)]
		int Zero { get; set; }

		[DefaultConstant(1.5)]
		double OnePointFive { get; set; }

		[DefaultConverted<DateTimeConverter>("1999-12-31")]
		DateTime EpochParty { get; set; }
	}

	public sealed class DateTimeConverter : IDefaultValueConverter
	{
		public object Convert(string defaultValueText)
		{
			return DateTime.Parse(defaultValueText);
		}
	}

	public sealed class TestViewModelProxy : ViewModelProxy<ITestViewModel>
	{
	}
}
