/** @format */

import React from 'react';
import { ButtonClass } from '../../../styles/button';
import { useKdmidUserEmbassies } from './kdmidUserEmbassiesHooks';

interface IKdmidUserEmbassiesProps {
  chatId: string;
}

export const KdmidUserEmbassies = ({ chatId }: IKdmidUserEmbassiesProps) => {
  const { embassies, onSubmit } = useKdmidUserEmbassies(chatId);

  return (
    <div className='w-80 absolute rounded-md left-1/2 top-1/3 transform -translate-x-1/2 -translate-y-1/3'>
      <h1 className='text-2xl font-bold mb-2'>Check appointments</h1>
      {embassies.map(embassy => (
        <div key={embassy.commandId} className='flex justify-end'>
          <button type='button' className={ButtonClass.Success} onClick={() => onSubmit(embassy.commandId)}>
            {embassy.city} {embassy.identifierId} ({embassy.attempts})
          </button>
        </div>
      ))}
    </div>
  );
};
