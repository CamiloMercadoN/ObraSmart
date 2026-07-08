export const validarRutChileno = (rutRecibido: string): boolean => {
  if (!rutRecibido) return false;

  const rutLimpio = rutRecibido.replace(/[^0-9kK]+/g, '').toUpperCase();
  if (rutLimpio.length < 2) return false;

  const cuerpo = rutLimpio.slice(0, -1);
  const dv = rutLimpio.slice(-1);

  let suma = 0;
  let multiplo = 2;

  for (let i = cuerpo.length - 1; i >= 0; i--) {
    suma += parseInt(cuerpo.charAt(i)) * multiplo;
    multiplo = multiplo < 7 ? multiplo + 1 : 2;
  }

  const dvEsperado = 11 - (suma % 11);
  const dvCalculado = dvEsperado === 11 ? '0' : dvEsperado === 10 ? 'K' : dvEsperado.toString();

  return dv === dvCalculado;
};

export const formatearRut = (rutRecibido: string): string => {
  if (!rutRecibido) return '';

  const valor = rutRecibido.replace(/[^0-9kK]+/g, '').toUpperCase();
  if (valor.length > 1) {
    const cuerpo = valor.slice(0, -1);
    const dv = valor.slice(-1);
    return `${cuerpo}-${dv}`;
  }
  return valor;
};
