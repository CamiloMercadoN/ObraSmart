const emailRegex = /^[^@\s]+@[^@\s]+\.[^@\s]+$/;

export const validarCorreo = (correo: string): boolean => {
  if (!correo) return true;
  return emailRegex.test(correo);
};
