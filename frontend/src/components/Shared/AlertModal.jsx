import React from 'react';
import { motion } from 'framer-motion';

export default function AlertModal({ 
  title = "Alert", 
  message, 
  confirmLabel = "OK", 
  cancelLabel = "Cancel", 
  onConfirm, 
  onCancel 
}) {
  return (
    <motion.div
      initial={{ opacity: 0 }}
      animate={{ opacity: 1 }}
      exit={{ opacity: 0 }}
      className="fixed inset-0 bg-black bg-opacity-70 flex items-center justify-center z-50"
    >
      <motion.div
        initial={{ scale: 0.8, opacity: 0 }}
        animate={{ scale: 1, opacity: 1 }}
        transition={{ duration: 0.2 }}
        className="bg-gray-200 border-4 border-black p-8 text-center max-w-sm shadow-2xl"
      >
        <h2 className="text-2xl font-bold mb-4 text-black">{title}</h2>
        <p className="text-black font-semibold mb-6">{message}</p>

        <div className="flex justify-center gap-4">
          {onCancel && (
            <motion.button
              whileHover={{ scale: 1.05 }}
              whileTap={{ scale: 0.95 }}
              onClick={onCancel}
              className="px-5 py-2 bg-gray-300 border-2 border-black text-black font-bold"
            >
              {cancelLabel}
            </motion.button>
          )}

          {onConfirm && (
            <motion.button
              whileHover={{ scale: 1.05 }}
              whileTap={{ scale: 0.95 }}
              onClick={onConfirm}
              className="px-5 py-2 bg-red-600 border-2 border-black text-white font-bold"
            >
              {confirmLabel}
            </motion.button>
          )}
        </div>
      </motion.div>
    </motion.div>
  );
}
