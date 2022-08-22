/* 
*   NatDevice OpenCV
*   Copyright (c) 2022 NatML Inc. All Rights Reserved.
*/

namespace NatML.Devices.Outputs {

    using System;
    using UnityEngine;
    using Unity.Collections.LowLevel.Unsafe;
    using OpenCVForUnity.CoreModule;
    using OpenCVForUnity.ImgprocModule;
    using OpenCVForUnity.UtilsModule;

    /// <summary>
    /// Camera device output that converts camera images into OpenCV matrices.
    /// </summary>
    public sealed class MatOutput : CameraOutput {

        #region --Client API--
        /// <summary>
        /// Matrix conversion options.
        /// </summary>
        public class ConversionOptions : PixelBufferOutput.ConversionOptions {
            /// <summary>
            /// Conversion format.
            /// The format MUST begin with `Imgproc.COLOR_RGBA2***`.
            /// </summary>
            public int format = 0;
        }

        /// <summary>
        /// OpenCV matrix containing the latest camera image.
        /// </summary>
        public Mat matrix => convertedMatrix.width() > 0 ? convertedMatrix : null;

        /// <summary>
        /// Get or set the matrix orientation.
        /// </summary>
        public ScreenOrientation orientation {
            get => pixelBufferOutput.orientation;
            set => pixelBufferOutput.orientation = value;
        }

        /// <summary>
        /// Get or set the conversion format.
        /// The format MUST begin with `COLOR_RGBA***`.
        /// </summary>
        public int format = 0;

        /// <summary>
        /// Create a Mat output.
        /// </summary>
        public MatOutput () {
            this.pixelBufferOutput = new PixelBufferOutput();
            this.frameMatrix = new Mat();
            this.convertedMatrix = new Mat();
        }

        /// <summary>
        /// Update the output with a new camera image.
        /// </summary>
        /// <param name="image">Camera image.</param>
        public override void Update (CameraImage image) => Update(image, null);

        /// <summary>
        /// Update the output with a new camera image.
        /// </summary>
        /// <param name="image">Camera image.</param>
        /// <param name="options">Conversion options.</param>
        public unsafe void Update (CameraImage image, ConversionOptions options) {
            pixelBufferOutput.Update(image, options);
            var (width, height) = (pixelBufferOutput.width, pixelBufferOutput.height);
            if (frameMatrix.width() != width || frameMatrix.height() != height)
                frameMatrix.create(height, width, CvType.CV_8UC4);
            MatUtils.copyToMat((IntPtr)pixelBufferOutput.pixelBuffer.GetUnsafeReadOnlyPtr(), frameMatrix);
            var code = options?.format ?? format;
            if (code != 0)
                Imgproc.cvtColor(frameMatrix, convertedMatrix, code);
            else
                frameMatrix.copyTo(convertedMatrix);
        }

        /// <summary>
        /// Dispose the Mat output and release resources.
        /// </summary>
        public override void Dispose () {
            pixelBufferOutput.Dispose();
            frameMatrix.Dispose();
            convertedMatrix.Dispose();
        }
        #endregion


        #region --Operations--
        private readonly PixelBufferOutput pixelBufferOutput;
        private readonly Mat frameMatrix;
        private readonly Mat convertedMatrix;
        #endregion
    }
}