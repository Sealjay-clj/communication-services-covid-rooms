// © Microsoft Corporation. All rights reserved.
import React from 'react';
import { Stack } from '@fluentui/react';
import {
  endCallContainerStyle,
  endCallTitleStyle,
  mainStackTokens,
  upperStackTokens,
} from './styles/EndCall.styles';

export interface TransferRoomProps {
  message: string;
}

export default (props: TransferRoomProps): JSX.Element => {

  return (
    <Stack verticalAlign="center" tokens={mainStackTokens} className={endCallContainerStyle}>
      <Stack tokens={upperStackTokens}>
        <div className={endCallTitleStyle}>{props.message}</div>
      </Stack>
    </Stack>
  );
};
