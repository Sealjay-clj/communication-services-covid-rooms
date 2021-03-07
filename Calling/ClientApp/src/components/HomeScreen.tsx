// © Microsoft Corporation. All rights reserved.
import React from 'react';
import { Stack, PrimaryButton, Icon, Image, IImageStyles } from '@fluentui/react';
import { VideoCameraEmphasisIcon } from '@fluentui/react-icons-northstar';
import heroSVG from '../assets/hero.svg';
import {
  imgStyle,
  containerTokens,
  listStyle,
  iconStyle,
  headerStyle,
  upperStackTokens,
  videoCameraIconStyle,
  buttonStyle,
  nestedStackTokens,
  upperStackStyle, listItemStyle
} from './styles/HomeScreen.styles';

export interface HomeScreenProps {
  startCallHandler(): void;
}

const imageStyleProps: IImageStyles = {
  image: {
    height: '100%',
    width: '100%'
  },
  root: {}
};

export default (props: HomeScreenProps): JSX.Element => {
  const iconName = 'SkypeCircleCheck';
  const imageProps = { src: heroSVG.toString() };
  const headerTitle = 'Video Mixer';
  const startCallButtonText = 'Enter Lobby';
  const listItems = [
    'Meet everyone in the lobby',
    'Mix with colleagues selected at random',
    'Receive conversation prompts',
    'Get to know people!',
    'Based on the Azure Communication Services video calling'
  ];
  return (
    <Stack horizontal horizontalAlign="center" verticalAlign="center" tokens={containerTokens}>
      <Stack className={upperStackStyle} tokens={upperStackTokens}>
        <div className={headerStyle}>{headerTitle}</div>
        <Stack tokens={nestedStackTokens}>
            <ul className={listStyle}>
                <li className={listItemStyle}>
                    <Icon className={iconStyle} iconName={iconName} /> {listItems[0]}
                </li>
                <li className={listItemStyle}>
                    <Icon className={iconStyle} iconName={iconName} /> {listItems[1]}
                </li>
                <li className={listItemStyle}>
                    <Icon className={iconStyle} iconName={iconName} /> {listItems[2]}
            </li>
            <li className={listItemStyle}>
                    <Icon className={iconStyle} iconName={iconName} /> {listItems[3]}
                </li>
                <li className={listItemStyle}>
                    <Icon className={iconStyle} iconName={iconName} /> {listItems[4]}{' '}
                    <a href="https://docs.microsoft.com/en-us/azure/communication-services/samples/calling-hero-sample?pivots=platform-web">sample</a>
                </li>
            </ul>
        </Stack>
        <PrimaryButton className={buttonStyle} onClick={props.startCallHandler}>
          <VideoCameraEmphasisIcon className={videoCameraIconStyle} size="medium" />
          {startCallButtonText}
        </PrimaryButton>
      </Stack>
      <Image
        alt="The intro image from the Azure Communication Services Calling sample app"
        className={imgStyle}
        styles={imageStyleProps}
        {...imageProps}
      />
    </Stack>
  );
};
