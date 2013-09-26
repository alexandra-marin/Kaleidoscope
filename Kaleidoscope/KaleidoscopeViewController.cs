using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.IO;
using MonoTouch.CoreGraphics;
using System.Linq;

namespace Kaleidoscope
{
	public partial class KaleidoscopeViewController : UIViewController
	{
		public UIDynamicAnimator Animator { get; private set; }
		UIScrollView imagesContainer;
		int imagePositionX = 0;
		int imagePositionY = 0;
		int imageWidth = 100;
		int imageHeight = 100;
		UITapGestureRecognizer tap;
		UISnapBehavior snap;

		public KaleidoscopeViewController () : base ("KaleidoscopeViewController", null)
		{
		}

		public override void DidReceiveMemoryWarning ()
		{
			base.DidReceiveMemoryWarning ();
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			Animator = new UIDynamicAnimator (View);
			CreateImagesContainer ();
		}

		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			return (toInterfaceOrientation != UIInterfaceOrientation.PortraitUpsideDown);
		}

		private void CreateImagesContainer ()
		{
			imagesContainer = new UIScrollView (View.Bounds);
			imagesContainer.SizeToFit (); //The size of the scroll needs to contain all its children
			imagesContainer.ContentSize = new SizeF (View.Bounds.Width, View.Bounds.Height); //Set the ContentSize smaller than the actual scroll's size, to enable scrolling behaviour 
			imagesContainer.AutoresizingMask = UIViewAutoresizing.All;
			View.Add (imagesContainer);

			LoadImages ();
		}

		private void LoadImages ()
		{
			//Get the photos from the Images folder and add them to the gallery
			string[] filePaths = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "/Images", "*.jpg");
			foreach (string path in filePaths) 
			{ 
				UIImageView imgView = new UIImageView ();
				imgView.Image = new UIImage (path);
				imgView.Frame = new RectangleF (imagePositionX, imagePositionY, imageWidth, imageHeight);

				AddTapRecognizer (imgView); 
				imagesContainer.Add (imgView);

				IncrementPositionVariables ();
			}
		}

		void IncrementPositionVariables ()
		{
			if (imagePositionX + imageWidth * 2 < View.Bounds.Width)
				imagePositionX = imagePositionX + imageWidth;
			else {
				imagePositionX = 0;
				imagePositionY = imagePositionY + imageHeight;
			}
		}

		void AddTapRecognizer (UIView imgView)
		{ 
			tap = new UITapGestureRecognizer (() =>  { AnimateImageShow (imgView); });
			imgView.AddGestureRecognizer (tap);
			imgView.UserInteractionEnabled = true; //So it will be able to register taps
		}

		void AnimateImageShow (UIView imgView)
		{ 
			//Remove hanging events 
			Animator.RemoveAllBehaviors ();  

			//Center and enlarge the selected image
			snap = new UISnapBehavior (imgView, imagesContainer.Center);
			snap.Action = () => { ScaleImage (imgView, 3); };
			Animator.AddBehavior (snap);
			imagesContainer.BringSubviewToFront (imgView);

			DropImages(imagesContainer.Subviews.Where (x => x != imgView).ToArray());
		}

		private void ScaleImage (UIView imgView, float factor)
		{
			CGAffineTransform transformation = CGAffineTransform.MakeIdentity ();
			transformation.Scale (factor, factor);
			imgView.Transform = transformation;
		}

		void DropImages (UIView[] imgsView)
		{ 
			var gravityBehavior = new UIGravityBehavior (imgsView);
			var collisionBehavior = new UICollisionBehavior (imgsView) {
				TranslatesReferenceBoundsIntoBoundary = true,
				CollisionMode = UICollisionBehaviorMode.Everything
			}; 

			Animator.AddBehaviors (gravityBehavior, collisionBehavior);
		}
	}
}

